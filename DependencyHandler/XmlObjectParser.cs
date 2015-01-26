using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DependencyHandling
{

    class XmlObjectParser
    {
        public static XmlObjectParser Default = new XmlObjectParser();

        public XmlObjectParser()
        {
        }

        // loads a file from disk
        public Project OpenProject(string filePath)
        {
            Project project = this.FileToObject(filePath) as Project;
            String directoryPath = Directory.GetParent(filePath).FullName;
            project.source.GetValue().location.SetValue(new FileLocation(directoryPath));
            return project;

        }

        // loads a bunch of objects from a file
        public object FileToObject(String fileName)
        {
            return this.StructureToObject(this.XmlToStructure(this.ReadFile(fileName)));
        }

        public object StructureToObject(XmlDocument document)
        {
            Stack<object> objects = new Stack<object>();
            XmlNode node = document.FirstChild;
            return this.parse(node);
        }

        private object parse(XmlNode node)
        {
            //Logger.Message("parsing node '" + node.Name + "'");
            string name = node.Name;
            if (this.IsRegisterableClass(name))
            {
                Type type = this.GetRegisteredClass(name);
                if (type == null)
                    throw new InvalidDataException("Attempted to reference class '" + name + "', which must be registered and is not registered");
                return this.parse(node, type);
            }
            else
            {
                throw new ArgumentException("Root node does not specify its type");
            }
        }

        // parses the given XmlNode into an object of the given type
        private object parse(XmlNode node, Type type)
        {
            //Logger.Message("parsing node '" + node.Name + "' to object type " + type);
            if (type == null)
            {
                throw new ArgumentException("Cannot parse an object into a null type");
            }

            string name = node.Name;
            // call the default constructor
            object item = null;
            // set a value for each property
            foreach (XmlNode childNode in node.ChildNodes)
            {
                object childItem = null;
                string childName = childNode.Name;
                Type childType = null;
                // determine the type of the property
                if (this.IsRegisterableClass(childName))
                {
                    childType = this.GetRegisteredClass(childName);
                    // This node just specifies the type of the parent node, so parse it as that type and return it
                    if (node.ChildNodes.Count != 1)
                    {
                        throw new InvalidDataException("Node " + node + " contains child node " + childNode + " which specifies class " + childType + " but the parent node contains multiple children (" + node.ChildNodes.Count + ")");
                    }
                    item = this.parse(childNode, childType);
                    this.assertType(item, type);
                    break;
                }
                // since we now know that we're going to set a property on the element, we now have to call the constructor
                if (item == null)
                {
                    if (childNode.ChildNodes.Count == 0)
                    {
                        item = new ConstantValue_Provider<string>(childNode.InnerText);
                        this.assertType(item, type);
                        //Logger.Message("done parsing " + node.Name);
                        return item;
                    }
                    ConstructorInfo constructor = type.GetConstructor(new Type[0]);
                    if (constructor == null)
                    {
                        throw new InvalidOperationException("No default constructor found for " + type);
                    }
                    else
                    {
                        item = constructor.Invoke(new object[0]);
                        this.assertType(item, type);
                    }
                }

                // the child type is controlled by the parent object
                childType = this.getPropertyType(type, childName);
                childItem = this.parse(childNode, childType);

                PropertyInfo propertyInfo = this.getProperty(type, childName);
                object[] parameters = new object[] { childItem };
                MethodInfo setter = propertyInfo.SetMethod;
                if (setter == null)
                {
                    throw new InvalidOperationException("No setter found for " + propertyInfo);
                }
                setter.Invoke(item, parameters);
            }

            // make sure that we called the constructor
            if (item == null)
            {
                ConstructorInfo constructor = type.GetConstructor(new Type[0]);
                if (constructor == null)
                {
                    throw new InvalidOperationException("No default constructor found for " + type);

                }
                else
                {
                    item = constructor.Invoke(new object[0]);
                    this.assertType(item, type);
                }

            }

            //Logger.Message("done parsing " + node.Name + " as " + item);
            this.assertType(item, type);
            return item;
        }

        private void assertType(object item, Type type)
        {
            if (item == null)
                return;
            Type returnType = item.GetType();
            if (Object.Equals(returnType, type))
                return;
            if (returnType.IsSubclassOf(type))
                return;
            if (returnType.GetInterfaces().Contains(type))
                return;

            throw new Exception("Attempted to return item " + item + ", which does not inherit from requested type " + type);
                    
        }

        // gets the type of a property
        private Type getPropertyType(Type parentType, string propertyName)
        {
            PropertyInfo property = this.getProperty(parentType, propertyName);
            if (property == null)
            {
                throw new InvalidOperationException("Property '" + propertyName + "' does not exist on '" + parentType + "'");
            }
            Type resultantType = property.PropertyType;
            //Logger.Message("Found property type '" + resultantType + "' for property '" + propertyName + "' of class '" + parentType + "'");
            return resultantType;
        }

        private PropertyInfo getProperty(Type parentType, string propertyName)
        {
            if (parentType == null)
            {
                throw new ArgumentException("Cannot get property '" + propertyName + "' of '" + parentType + "'");
            }
            foreach (PropertyInfo propertyInfo in parentType.GetProperties())
            {
                if (String.Equals(propertyInfo.Name, propertyName))
                {
                    return propertyInfo;
                }
            }
            throw new ArgumentException("Type " + parentType + " has no '" + propertyName + "' property");
        }

        // tells whether this name is allowed to be a class name
        public Boolean IsRegisterableClass(string name)
        {
            return (name != null && name.Length > 0 && Char.IsUpper(name[0]));
        }
        public Boolean IsRegisteredClass(string name)
        {
            return (this.IsRegisterableClass(name) && this.registeredTypes.ContainsKey(name));
        }
        public Type GetRegisteredClass(string name)
        {
            if (this.IsRegisterableClass(name))
            {
                if (this.registeredTypes.ContainsKey(name))
                    return this.registeredTypes[name];
            }
            throw new InvalidOperationException("No class registered for name: " + name);
        }
        public void RegisterClass(string name, Type type)
        {
            if (!this.IsRegisterableClass(name))
            {
                throw new ArgumentException("Type name '" + name + "' does not fit the naming conventions required to register as a class name");
            }
            if (this.IsRegisteredClass(name))
            {
                throw new InvalidOperationException("Type name '" + name + "' is already registered to '" + this.registeredTypes[name] + " and cannot be registered to " + type);
            }
            this.registeredTypes[name] = type;
        }

        public XmlDocument XmlToStructure(String xml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            return document;
        }
        public String ReadFile(String fileName)
        {
            StreamReader reader = new StreamReader(fileName);
            return reader.ReadToEnd();
        }

        public Dictionary<string, Type> registeredTypes = new Dictionary<string,Type>();
        
    }



    // TODO delete the ParseableList class and figure out how else to parse a list
    public class ParseableList<T> : List<T>, ValueProvider<List<T>>
    {
        public T item
        {
            set
            {
                this.Add(value);
            }
        }

        public List<T> GetValue()
        {
            return this;
        }

        public void SetValue(List<T> value)
        {
            this.Clear();
            this.AddRange(value);
        }

    }

}
