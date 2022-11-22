using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPTools
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class DuckCreationRevitCommand : IExternalCommand
    {
        static AddInId appId = new AddInId(new Guid("65448805-9EF4-4FF1-B36F-D1E404377255"));
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            if (doc.IsDocumentFamily())
                return Result.Failed;
            DuckCreationView dcv = new DuckCreationView(commandData);
            dcv.ShowDialog();
            return Result.Succeeded;
        }
    }
}
