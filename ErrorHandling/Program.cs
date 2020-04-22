using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ErrorHandling
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"E:\Project\QuickTestAsync\ErrorHandling\data\2.xml";
            Task task = Task.Factory.StartNew(() => Import(path));

            try
            {
                task.Wait();
            }
            catch (AggregateException errors)
            {
                //Console.WriteLine("{0} : {1}", errors.GetType().Name, errors.Message);
                //foreach (Exception error in errors.Flatten().InnerExceptions)
                //{
                //    Console.WriteLine("{0} : {1}", error.GetType().Name, error.Message);
                //}
                errors.Handle(IgnoreXmlError);
            }
        }

        static void Import(string fullName)
        {
            XElement doc = XElement.Load(fullName);
        }

        private static bool IgnoreXmlError(Exception arg)
        {
            return (arg.GetType() == typeof(XmlException));
        }
    }
}
