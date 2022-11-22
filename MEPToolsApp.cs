using System;
using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace MEPTools
{
    public class MEPToolsApp : IExternalApplication
    {
        static AddInId appId = new AddInId(new Guid("DA5C7994-5A3A-4327-9F08-DE38BF47EE9D"));
        public Result OnStartup(UIControlledApplication app)
        {    
            string folderPath3 = @"C:\ProgramData\Autodesk\Revit\Addins\2022\MEPTools"; 
            string dll = Path.Combine(folderPath3, @"MEPTools.dll");            
            RibbonPanel AdditionalTools = app.CreateRibbonPanel("MEP Tools");            
            PushButton duck = (PushButton)AdditionalTools.AddItem(new PushButtonData("Create Duck", "Create Duck", dll, "MEPTools.DuckCreationRevitCommand"));
            duck.ToolTip = "Создание обхода трубопровода с привязкой или без";
            if (File.Exists(Path.Combine(folderPath3, "duck.png")))
                duck.LargeImage = new BitmapImage(new Uri(Path.Combine(folderPath3, "duck.png"), UriKind.Absolute));            
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication app)
        {
            return Result.Succeeded;
        }
    }
}
