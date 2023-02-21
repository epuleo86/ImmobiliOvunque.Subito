using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ImmobiliOvunque.Subito
{
    public class StreamImporter
    {
        [Key]
        [Column("idgestionale")]
        public int IdGestionale { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("start")]
        public DateTime Start { get; set; }

        [Column("end")]
        public DateTime? End { get; set; }

        [Column("agenzie")]
        public int? Agenzie { get; set; }

        [Column("immobili")]
        public int? Immobili { get; set; }
    }
}
