using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ImmobiliOvunque.Subito
{
    class Program
    {
        private const string URL_LISTING = "https://www.subito.it/annunci-italia/vendita/immobili/?o={0}";
        private const int PAGES = 30;
        private const int STREAM_IMPORTER_ID = -100;

#if DEBUG
        private const string CONNECTION_STRING = "SERVER=172.24.0.250;DATABASE=IMMOBILISVIL;UID=immobili;PASSWORD=Xray8888$$!;convert zero datetime=True;charset=utf8;Allow User Variables=True;MinimumPoolSize=10;maximumpoolsize=300;";
#else
        private const string CONNECTION_STRING = "SERVER=172.24.0.251;DATABASE=PRODUZIONE;UID=immobili;PASSWORD=Xray8888$$!;convert zero datetime=True;charset=utf8;Allow User Variables=True;MinimumPoolSize=10;maximumpoolsize=300;";
#endif

        private HtmlWeb web;
        private Random random = new Random((int)DateTime.Now.Ticks);
        private HttpClient client;
        private IOContext context;

        private void LogException(Exception exception, object data = null)
        {
            string fullFilePath = Path.Combine(Environment.CurrentDirectory, DateTime.Now.ToString("yyyy-MM-dd") + "_log.txt");
            var n = Environment.NewLine;

            string exc = exception.GetType() + ": " + exception.Message + n + exception.StackTrace + n;

            if (exception.InnerException != null)
                exc += $"\r\nInner exception: {exception.InnerException.Message}";

            if (exception is DbUpdateException)
            {
                var dbUpdateException = exception as DbUpdateException;
                foreach (var eve in dbUpdateException.Entries)
                {
                    exc += $"\r\nEntity of type {eve.Entity.GetType().Name} in state {eve.State} could not be updated";
                }
            }

            if (data != null)
                exc += $"\r\nData: {JsonConvert.SerializeObject(data)}";

            File.AppendAllText(fullFilePath, DateTime.Now.ToString() + n + exc);

            Console.WriteLine(exception.Message);
        }

        private bool CheckSingleIsAzienda(HtmlNode documentNode)
        {
            var user = documentNode.SelectNodes("//div[@class='user-details-name jsx-4049635516']/span");

            return user?.Any() == true && user.First().InnerText.ToLower() == "azienda";
        }

        private string NormalizeProvince(string province)
        {
            if (string.IsNullOrEmpty(province))
                return province;

            if (province == "Massa-Carrara")
                return "Massa e Carrara";

            return province;
        }

        private async Task<bool> ParseSingle(string url)
        {
            if (context.Annunci.Any(s => s.Link == url))
                return false;

            AnnuncioSubito annuncio = null;

            try
            {
                var doc = web.Load(url);
                var documentNode = doc.DocumentNode;

                if (CheckSingleIsAzienda(documentNode))
                    return false;

                var script = documentNode.Descendants().Where(n => n.Name == "script" && n.Id == "__NEXT_DATA__").FirstOrDefault().InnerText;

                string city;
                string province;
                string name;
                string phone = null;

                bool showNumber = documentNode.SelectSingleNode("//button[@aria-label='mostra numero di telefono']") != null;

                if (!string.IsNullOrEmpty(script) && showNumber)
                {
                    var root = JsonConvert.DeserializeObject<RootScriptObject>(script);

                    city = root.props.state.detail.item.geo.town.value;
                    province = root.props.state.detail.item.geo.city.value;
                    name = root.props.pageProps.advertiserProfile.username;
                    var urn = root.props.state.detail.item.urn;

                    string telephoneUrl = "https://www.subito.it/hades/v1/contacts/ads/" + urn;

                    var response = await client.GetAsync(telephoneUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();

                        phone = JsonConvert.DeserializeObject<RootAdsObject>(responseString)?.phone_number;
                    }
                }
                else
                {
                    var breadcrumbs = documentNode.SelectNodes("//ol[@class='index-module_container__rA-Ps']/li");

                    city = breadcrumbs.Last().InnerText;
                    province = breadcrumbs[breadcrumbs.Count - 2].InnerText;
                    province = province.Substring(0, province.IndexOf("(")).Trim();

                    name = doc.DocumentNode.SelectSingleNode("//div[@class='PrivateUserProfileBadge-module_text_container__jPb1q']/h6")?.InnerText;
                }

                if (string.IsNullOrEmpty(name))
                    name = doc.DocumentNode.SelectSingleNode("//p[@class='index-module_sbt-text-atom__ed5J9 index-module_token-button__eMeQT index-module_size-small__XFVFl index-module_weight-semibold__MWtJJ']")?.InnerText;

                string title = documentNode.SelectSingleNode("//h1").InnerText;

                annuncio = new AnnuncioSubito()
                {
                    Data = DateTime.Now.Date,
                    Creatore = name.ClearText() ?? string.Empty,
                    Link = url,
                    Localita = city,
                    Numero = phone.ClearText() ?? string.Empty,
                    Ora = DateTime.Now.TimeOfDay,
                    Provincia = NormalizeProvince(province),
                    Titolo = title.ClearText(),
                    UltimaScansione = DateTime.Now.Date,
                };

                context.Annunci.Add(annuncio);

                await context.SaveChangesAsync();

                await Task.Delay(TimeSpan.FromMilliseconds(random.Next(3000, 15000)));

                return true;
            }
            catch (Exception ex)
            {
                LogException(ex, data: new { annuncio = annuncio, link = url });
            }

            return false;
        }

        private void StartStreamImporter()
        {
            try
            {
                var streamImporter = context.StreamImporters.FirstOrDefault(s => s.IdGestionale == STREAM_IMPORTER_ID);

                if (streamImporter == null)
                {
                    streamImporter = new StreamImporter()
                    {
                        IdGestionale = STREAM_IMPORTER_ID,
                    };

                    context.Add(streamImporter);
                }

                streamImporter.End = null;
                streamImporter.Start = DateTime.Now;
                streamImporter.Status = 0;

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void EndStreamImporter(int status, int immobili)
        {
            try
            {
                var streamImporter = context.StreamImporters.FirstOrDefault(s => s.IdGestionale == STREAM_IMPORTER_ID);

                if (streamImporter == null)
                    return;

                streamImporter.End = DateTime.Now;
                streamImporter.Status = status;
                streamImporter.Immobili = immobili;

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task Scan()
        {
            Console.WriteLine("START SCAN");

            foreach (var annuncio in context.Annunci.Where(s => s.Data != DateTime.Now.Date && (s.UltimaScansione == null || s.UltimaScansione.Value.AddDays(5) < DateTime.Now.Date)).ToList())
            {
                try
                {
                    Console.WriteLine("SCAN URL: " + annuncio.Link);

                    var doc = web.Load(annuncio.Link);

                    if (web.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var documentNode = doc.DocumentNode;

                        if (CheckSingleIsAzienda(documentNode))
                        {
                            context.Annunci.Remove(annuncio);
                        }
                        else
                        {
                            annuncio.UltimaScansione = DateTime.Now;
                        }
                    }
                    else
                    {
                        context.Annunci.Remove(annuncio);
                    }

                    context.SaveChanges();

                    await Task.Delay(TimeSpan.FromMilliseconds(random.Next(3000, 15000)));
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }

            Console.WriteLine("END SCAN");
        }

        private async void Start()
        {
            web = new HtmlWeb();
            client = new HttpClient();
            context = new IOContext(CONNECTION_STRING);

            Console.WriteLine("START SUBITO");

            while (true)
            {
                StartStreamImporter();
                bool errors = false;
                int immobili = 0;

                for (int i = 1; i <= PAGES; i++)
                {
                    try
                    {
                        string listing = string.Format(URL_LISTING, i);
                        var doc = web.Load(listing);
                        Console.WriteLine("LISTING: " + listing);
                        foreach (var node in doc.DocumentNode.SelectNodes("//div[contains(@class, 'item-card item-card--big')]"))
                        {
                            try
                            {
                                string url = node.SelectSingleNode("./a").GetAttributeValue("href", string.Empty);

                                Console.WriteLine("URL: " + url);

                                bool isAgency = node.SelectSingleNode(".//div[@class='index-module_link__yhlFw index-module_large__kJn3N']")?.ChildNodes?.Count > 0;

                                if (isAgency)
                                    continue;

                                if (await ParseSingle(url))
                                    immobili++;
                            }
                            catch (Exception ex)
                            {
                                LogException(ex);
                                errors = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                        errors = true;
                    }
                }

                EndStreamImporter(!errors ? 0 : -1, immobili);
                Console.WriteLine("END SUBITO");

                await Scan();

                await Task.Delay(TimeSpan.FromHours(1));
            }
        }

        static void Main(string[] args)
        {
            Console.SetOut(new PrefixedWriter());
            new Program().Start();
            Console.ReadLine();
        }
    }
}
