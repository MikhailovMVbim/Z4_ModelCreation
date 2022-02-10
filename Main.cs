using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z4_ModelCreation
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Получаем доступ к Revit, активному документу, базе данных документа
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // запускаем транзакцию
            Transaction t = new Transaction(doc);
            t.Start("Create model");

            // получаем уровни
            Level levelBase = GetLevel(doc, "Уровень 1");
            Level levelTop = GetLevel(doc, "Уровень 2");

            // создаем стены  с заданными габаритами на любом уровне и до любого уровня
            var walls = CreateWalls(doc, levelBase, levelTop, 18000, 6000);

            t.Commit();
            return Result.Succeeded;
        }

        private static double mmToFeet(double lenght)
        {
            return UnitUtils.ConvertToInternalUnits(lenght, UnitTypeId.Millimeters);
        }

        private static List<Wall> CreateWalls(Document doc, Level levelBase, Level levelTop, double lenght, double width)
        {
            double dx = mmToFeet(lenght) * 0.5;
            double dy = mmToFeet(width) * 0.5;
            List<XYZ> wallPoints = new List<XYZ>();
            wallPoints.Add(new XYZ(-dx, -dy, 0));
            wallPoints.Add(new XYZ(-dx, dy, 0));
            wallPoints.Add(new XYZ(dx, dy, 0));
            wallPoints.Add(new XYZ(dx, -dy, 0));
            wallPoints.Add(wallPoints[0]);
            // список стен
            List<Wall> walls = new List<Wall>();
            // создаем стены на заданном уровне, высотой до требуемого уровня
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(wallPoints[i], wallPoints[i + 1]);
                Wall wall = Wall.Create(doc, line, levelBase.Id, false);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(levelTop.Id);
                walls.Add(wall);
            }
            return walls;
        }

        private static Level GetLevel (Document doc, string levelName)
        {
            //получаем все уровни
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .Where(l => l.Name.Equals(levelName))
                .FirstOrDefault();
        }
    }
}
