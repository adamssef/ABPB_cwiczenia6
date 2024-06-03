using System;
using System.Collections.Generic;

namespace Medical.Models;

public partial class Medicament
{
    public int IdMedicament { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Type { get; set; }

    public virtual ICollection<PrescriptionMedicament> PrescriptionMedicaments { get; set; } = new List<PrescriptionMedicament>();
}
