using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPTools
{
    public class DuckCreationModel
    {
        public static DuckCreationViewModel viewModel { get; set; }
        private ExternalCommandData commandData { get => viewModel.commandData; }
        private UIApplication uiapp
        {
            get => commandData.Application;
        }
        private UIDocument uidoc
        {
            get => uiapp.ActiveUIDocument;
        }
        private Document doc
        {
            get => uidoc.Document;
        }
        private double offset = viewModel.usedOffset;
        private double angle = viewModel.usedAngle;
        private bool isUp = viewModel.IsUp;
        private bool isCyclic = viewModel.IsCyclic;

        private Reference firstRef;
        private Reference secondRef;

        XYZ p1;
        XYZ p2;

        private Connector c11;
        private Connector c21;
        private Connector c22;
        private Connector c32;

        private Connector contr_c11;
        private Connector contr_c21;
        private Connector contr_c22;
        private Connector contr_c32;

        private ElementId pipeTypeId;
        private ElementId levelId;
        private MEPCurve SelectPoints()
        {
            firstRef = uidoc.Selection.PickObject(ObjectType.Element, new Filter(), "Выберите точку на первом элементе");
            secondRef = uidoc.Selection.PickObject(ObjectType.Element, new Filter(), "Выберите вторую точку на том же элементе");
            MEPCurve m1 = doc.GetElement(firstRef) as MEPCurve;
            MEPCurve m2 = doc.GetElement(secondRef) as MEPCurve;
            if (m1.Id.IntegerValue != m2.Id.IntegerValue)
                return null;
            return m1;
        }
        private MEPCurve SelectPointsWithSnap()
        {
            firstRef = uidoc.Selection.PickObject(ObjectType.Element, new Filter(), "Выберите элемент для построения обхода");
            p1 = uidoc.Selection.PickPoint((ObjectSnapTypes)1023, "Укажите первую точку");
            p2 = uidoc.Selection.PickPoint((ObjectSnapTypes)1023, "Укажите вторую точку");
            return doc.GetElement(firstRef) as MEPCurve;
        }
        private XYZ[] GetBreakPoints(MEPCurve mepCurve)
        {
            XYZ[] points = new XYZ[2] { null, null };
            ConnectorSet cs = mepCurve.ConnectorManager.Connectors;
            foreach (Connector connector in cs)
            {
                if (connector.ConnectorType != ConnectorType.End) continue;
                if (points[0] == null)
                {
                    points[0] = connector.Origin;
                    continue;
                }
                points[1] = connector.Origin;
            }
            Line line = Line.CreateBound(points[0], points[1]);
            XYZ bp1 = null, bp2= null;
            if (viewModel.HasSnap)
            {
                bp1 = line.Project(p1).XYZPoint;
                bp2 = line.Project(p2).XYZPoint;
            }
            else 
            {
                bp1 = line.Project(firstRef.GlobalPoint).XYZPoint;
                bp2 = line.Project(secondRef.GlobalPoint).XYZPoint;
            }            
            points[0] = bp1;
            points[1] = bp2;
            return points;
        }
        private MEPCurve CreateNewPipe(MEPCurve m1, XYZ[] breakPoints)
        {
            ElementId secondPipeId = PlumbingUtils.BreakCurve(doc, m1.Id, breakPoints[0]);
            ElementId thirdPipeId = null;
            try 
            { 
                thirdPipeId = PlumbingUtils.BreakCurve(doc, m1.Id, breakPoints[1]);
            }
            catch 
            { 
                thirdPipeId = PlumbingUtils.BreakCurve(doc, secondPipeId, breakPoints[1]);
            }
            MEPCurve newPipe = null;
            MEPCurve m2 = doc.GetElement(secondPipeId) as MEPCurve;
            MEPCurve m3 = doc.GetElement(thirdPipeId) as MEPCurve;
            List<MEPCurve> mcs = new List<MEPCurve>() { m1, m2, m3 };
            int i = 3;
            newPipe = SelectNewPipe(mcs, breakPoints);
            i = mcs.IndexOf(newPipe);
            mcs.RemoveAt(i);
            foreach (MEPCurve m in mcs)
            {
                foreach (Connector c in m.ConnectorManager.Connectors)
                {
                    if (c.ConnectorType != ConnectorType.End) continue;
                    if (c.Origin.IsAlmostEqualTo(c21.Origin))
                    {
                        c11 = c;
                    }
                    else if (c.Origin.IsAlmostEqualTo(c22.Origin))
                    {
                        c32 = c;
                    }
                }
            }
            Parameter heightParam = newPipe.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM);
            double height = heightParam.AsDouble();
            if (isUp)
            {
                heightParam.Set(height + offset / 304.8);
            }
            else
            {
                heightParam.Set(height - offset / 304.8);
            }
            return newPipe;
        }
        private MEPCurve SelectNewPipe(List<MEPCurve> mepCurves, XYZ[] breakPoints)
        {
            MEPCurve newPipe = null;
            foreach (MEPCurve m in mepCurves)
            {
                bool a = false, b = false;
                foreach (Connector c in m.ConnectorManager.Connectors)
                {
                    if (c.ConnectorType != ConnectorType.End) continue;
                    if (c.Origin.IsAlmostEqualTo(breakPoints[0]) || c.Origin.IsAlmostEqualTo(breakPoints[1]))
                    {
                        if (!a)
                        {
                            a = true;
                            c21 = c;
                        }
                        else
                        {
                            b = true;
                            c22 = c;
                        }
                    }
                }
                if (a && b)
                {
                    newPipe = m;                    
                    break;
                }
            }
            return newPipe;
        }
       
        private void CorrectNewConnectorsByAngle()
        {
            XYZ A = c21.Origin;
            XYZ B = c22.Origin;
            XYZ C = c11.Origin;
            XYZ D = c32.Origin;
            XYZ E = null;
            XYZ F = null;
            double alpha = angle * (Math.PI / 180);
            E = A + ((B - A) / B.DistanceTo(A) * C.DistanceTo(A) * Math.Cos(alpha) / Math.Sin(alpha));
            F = B + ((A - B) / B.DistanceTo(A) * D.DistanceTo(B) * Math.Cos(alpha) / Math.Sin(alpha));
            c21.Origin = E;
            c22.Origin = F;
        }
        private void FindContrConnectors(Pipe p1, Pipe p2)
        {
            foreach (Connector c in p1.ConnectorManager.Connectors)
            {
                if (c.Origin.IsAlmostEqualTo(c11.Origin))
                {
                    contr_c11 = c;
                }
                else
                {
                    contr_c21 = c;
                }
            }
            foreach (Connector c in p2.ConnectorManager.Connectors)
            {
                if (c.Origin.IsAlmostEqualTo(c32.Origin))
                {
                    contr_c32 = c;
                }
                else
                {
                    contr_c22 = c;
                }
            }
        }
        private void CreateElbows()
        {
            doc.Create.NewElbowFitting(contr_c11, c11);
            doc.Create.NewElbowFitting(c21, contr_c21);
            doc.Create.NewElbowFitting(c22, contr_c22);
            doc.Create.NewElbowFitting(c32, contr_c32);
        }
        public void CreateDuck()
        {
            do
            {
                try
                {
                    MEPCurve m1 = null;
                    if (viewModel.HasSnap)
                    {
                        m1 = SelectPointsWithSnap();
                    }
                    else
                    {
                        m1 = SelectPoints();
                    }
                    if (m1 == null)
                        continue;
                    pipeTypeId = m1.GetTypeId();
                    levelId = m1.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM).AsElementId();
                    XYZ[] breakPoints = GetBreakPoints(m1);
                    using (TransactionGroup tg = new TransactionGroup(doc, "Создание обхода"))
                    {
                        tg.Start();
                        using (Transaction t = new Transaction(doc, "Создание обхода"))
                        {
                            t.Start();
                            MEPCurve newPipe = CreateNewPipe(m1, breakPoints);
                            t.Commit();
                            t.Start();
                            CorrectNewConnectorsByAngle();
                            t.Commit();
                            t.Start();
                            Pipe p1 = Pipe.Create(doc, pipeTypeId, levelId, c11, c21);
                            Pipe p2 = Pipe.Create(doc, pipeTypeId, levelId, c22, c32);
                            FindContrConnectors(p1, p2);
                            CreateElbows();
                            t.Commit();
                        }
                        tg.Assimilate();
                    }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    break;
                }
            }
            while (isCyclic);
        }
    }
    public class Filter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (!(elem is MEPCurve))
                return false;
            return true;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

}
