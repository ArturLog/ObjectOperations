using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Xml.XPath;
using System.Xml;

namespace Labkii3Platformy
{
    public class MainProgram
    {

        static  List<Car> myCars = new List<Car>()
        {
            new Car("E250", new Engine(1.8, 204, "CGI"), 2009),
            new Car("E350", new Engine(3.5, 292, "CGI"), 2009),
            new Car("A6", new Engine(2.5, 187, "FSI"), 2012),
            new Car("A6", new Engine(2.8, 220, "FSI"), 2012),
            new Car("A6", new Engine(3.0, 295, "TFSI"), 2012),
            new Car("A6", new Engine(2.0, 175, "TDI"), 2011),
            new Car("A6", new Engine(3.0, 309, "TDI"), 2011),
            new Car("S6", new Engine(4.0, 414, "TFSI"), 2012),
            new Car("S8", new Engine(4.0, 513, "TFSI"), 2012)

        };
        public static void LinQ()
        {
            var query = from car in myCars
                        where car.model == "A6"
                        select new
                        {
                            engineType = car.motor.model == "TDI" ? "diesel" : "petrol", //jesli true to diesel w innym przypadku petrol
                            hppl = car.motor.horsePower / car.motor.displacement
                        };
            foreach (var carElem in query)
            {
                Console.WriteLine(carElem.ToString());
            }
            var groupQuery = from car in query
                        group car by car.engineType into carGroup
                        select new
                        {
                            EngineType = carGroup.Key,
                            avgHppl = carGroup.Select(car => car.hppl).Average()
                        };
            foreach (var carGroup in groupQuery)
            {
                
                Console.WriteLine($"Engine Type: {carGroup.EngineType}, Average hppl: {carGroup.avgHppl}");
            }


        }
        public static void serializeIDeserialize()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Car>), new XmlRootAttribute("cars"));
            var filename = "CarsCollection.xml";
            var currentDirectory = Directory.GetCurrentDirectory();
            var carFilepath = Path.Combine(currentDirectory, filename);

            using (var stream = new FileStream(carFilepath, FileMode.Create))
            {
                serializer.Serialize(stream, myCars);
            }

            using (var stream = new FileStream(carFilepath, FileMode.Open))
            {
                var cars = (List<Car>)serializer.Deserialize(stream);
                foreach (var car in cars)
                {
                    Console.WriteLine($"Model: {car.model}");// zdeserializowane 
                }
            }

        }
        public static void XPath()
        {
            XElement rootNode = XElement.Load("CarsCollection.xml");
            double avgHp = (double)rootNode.XPathEvaluate("sum(//car/engine[@model!=\"TDI\"]/horsePower) div count(//car/engine[@model!=\"TDI\"]/horsePower)") ;
            Console.WriteLine($"Average: {avgHp}");//avg

            IEnumerable<XElement> models = rootNode.XPathSelectElements("//car[following-sibling::car/model = model]"); //bez powtorzen
           
        }
        private static void createXmlFromLinq(List<Car> myCars)
        {
            IEnumerable<XElement> nodes = from car in myCars select new XElement("car",
                new XElement("model", car.model),
                new XElement("engine",
                    new XElement("displacement", car.motor.displacement),
                    new XElement("horsePower", car.motor.horsePower)),
                    new XAttribute("model", car.motor.model),
                new XElement("year", car.year)
                
                );

            XElement rootNode = new XElement("cars", nodes); //create a root node to contain the query results
            rootNode.Save("CarsFromLinq.xml");
        }
        private static void LinqXml()
        {
            XElement template = XElement.Load("template.html");
            XNamespace xhtml = "http://www.w3.org/1999/xhtml";
            XElement body = template.Element(xhtml + "body");  //standardowy namespace URI dla XHTML
            
            IEnumerable<XElement> tableRows = from car in myCars select new XElement("tr", //tworzenie tabeli z 5 kolumnami
                        new XElement("td", car.model),
                        new XElement("td", car.motor.displacement),
                        new XElement("td", car.motor.horsePower),
                        new XElement("td", car.motor.model),
                        new XElement("td",car.year)
                    );
            body.Add(new XElement("table", tableRows)); //dodanie do body tabeli
            template.Save("template.html"); //nadpisanie template,
        }
        private static void modify()
        {
            XElement carsCollection = XElement.Load("CarsCollection.xml");

            foreach (XElement motor in carsCollection.Descendants("engine")) // zwraca wszystkich potomkow o nazwie engine 
            {
                
                var elem = motor.Element("horsePower");
                elem.Name = "hp"; //zmieniamy nazwe na hp
            }
            foreach(XElement car in carsCollection.Elements("car"))//zwraca bezpośrednie elementy Car
            {
                var elem = car.Element("year");
                car.Element("model").Add(new XAttribute("year", elem.Value));
                elem.Remove();
                
            }
            carsCollection.Save("modify.xml");
        }
        static void Main(string[] args)
        {
            LinQ();
            serializeIDeserialize();
            XPath();
            createXmlFromLinq(myCars);
            LinqXml();
            modify();
        }

    }

    
    [XmlType("car")]
    public class Car
    {
        public int year;
        
        public string model;
        [XmlElement(ElementName = "engine")]
        public Engine motor;

        public Car(string model, Engine engine, int year)
        {
            this.model = model;
            this.year = year;
            this.motor = engine;
        }
        public Car() { }
        
    }
    public class Engine
    {
        public double displacement;
        public double horsePower;
        [XmlAttribute("model")]
        public string model;
        public Engine() { }
        public Engine(double displacement, double horsePower, string model)
        {
            this.displacement = displacement;
            this.horsePower = horsePower;
            this.model = model;
        }
        
    }
   
}
