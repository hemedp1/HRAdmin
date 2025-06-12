using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRAdmin.Components
{
    public class AccidentRequired
    {
        public DateTime DateReport { get; set; }
        public string DriverInternal { get; set; }
        public string IndexNo { get; set; }
        public string Dept { get; set; }
        public string Car { get; set; }
        public string Destination { get; set; }
        public string DriverExternal { get; set; }
        public int NoofVehicle { get; set; }
        public string PlatNo { get; set; }
        public string VehicleType { get; set; }
        public string InsuranceClass { get; set; }
        public string InsuranceComp { get; set; }
        public string PolicyNo { get; set; }
        public string Address { get; set; }
        public DateTime DateofAccident { get; set; }
        public string Place {  get; set; }
        public DateTime Time { get; set; }
        public string PM { get; set; }
        public string PoliceStation { get; set; }
        public string ReportNo { get; set; }
        public string Remarks { get; set; }
        public int Tel { get; set; }
        public int IC { get; set; }
        public string ReportID { get; set; }
        public TypeVarBinarySchemaImporterExtension Attachment { get; set; }
        public string Explanation { get; set; }
        public string CheckStatus { get; set; }
        public string CheckedBy { get; set; }
        public DateTime DateCheck { get; set; }
        public string ApproveStatus { get; set; }
        public string ApproveBy { get; set; }
        public DateTime DateApprove { get; set; }

    }
}
