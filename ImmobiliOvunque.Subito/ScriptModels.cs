using System;
using System.Collections.Generic;
using System.Text;

namespace ImmobiliOvunque.Subito
{
    public class RootAdsObject
    {
        public string phone_number { get; set; }
        public bool force { get; set; }
    }

    public class RootScriptObject
    {
        public Props props { get; set; }
    }

    public class Props
    {
        public State state { get; set; }
        public Pageprops pageProps { get; set; }
    }

    public class State
    {
        public Detail detail { get; set; }
    }

    public class Detail
    {
        public Item item { get; set; }
    }

    public class Item
    {
        public string urn { get; set; }
        public Geo geo { get; set; }
    }

    public class Geo
    {
        public City city { get; set; }
        public Town town { get; set; }
    }

    public class City
    {
        public string value { get; set; }
    }

    public class Town
    {
        public string value { get; set; }
    }

    public class Pageprops
    {
        public Advertiserprofile advertiserProfile { get; set; }
    }

    public class Advertiserprofile
    {
        public string username { get; set; }
    }
}
