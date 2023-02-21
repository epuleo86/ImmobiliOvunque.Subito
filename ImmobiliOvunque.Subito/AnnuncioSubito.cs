using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ImmobiliOvunque.Subito
{
    public class AnnuncioSubito
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("creatore")]
        public string Creatore { get; set; }

        [Column("numero")]
        public string Numero { get; set; }

        [Column("titolo")]
        public string Titolo { get; set; }

        [Column("localita")]
        public string Localita { get; set; }

        [Column("provincia")]
        public string Provincia { get; set; }

        [Column("data")]
        public DateTime Data { get; set; }

        [Column("ora")]
        public TimeSpan Ora { get; set; }

        [Column("link")]
        public string Link { get; set; }

        [Column("ultima_scansione")]
        public DateTime? UltimaScansione { get; set; }
    }
}
