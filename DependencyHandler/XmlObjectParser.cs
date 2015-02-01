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

        public bool ProjectExists(string filePath)
        {
            if (File.Exists(filePath + "\\" + this.projectFileName))
                return true;
            return false;
        }
        // loads a file from disk
        public Project OpenProject(string filePath)
        {
            filePath = filePath + "\\" + this.projectFileName;
            Project project = this.FileToObject(filePath) as Project;
            String directoryPath = Directory.GetParent(filePath).FullName;
            project.source.GetValue().location = new FileLocation(directoryPath);
            project.parser = this;
            return project;
        }

        public void ReloadProject(string filePath, Project project)
        {
            XmlDocument xml = this.XmlToStructure(this.ReadFile(filePath + "\\" + this.projectFileName));
            this.UpdateObject(project, xml);
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

        // updates an object based on what's in the given file
        public void UpdateObject(object item, XmlNode node)
        {
            this.parse(node, new ObjectDescriptor(item));
        }

        private object parse(XmlNode node)
        {
            //Logger.Message("parsing node '" + node.Name + "'");
            object item = this.NewInstance(node.Name, null);
            return this.parse(node, new ObjectDescriptor(item));
        }

        // parses the given XmlNode into an object of the given type
        private object parse(XmlNode node, ObjectDescriptor objectDescriptor)
        {
            object item = objectDescriptor.Item;
            //if (item == null)
            //    throw new ArgumentException("Cannot fill in properties of a null object");

            string name = node.Name;
            // set a value for each property
            foreach (XmlNode childNode in node.ChildNodes)
            {
                object childItem = null;
                string childName = childNode.Name;
                // determine the type of the property
                if (this.IsRegisterableClass(childName))
                {
                    // This node just specifies the type of the parent node, so parse it as that type and return it
                    if (node.ChildNodes.Count != 1)
                        throw new InvalidDataException("Node " + node.Name + " contains child node " + childNode + " which specifies provider name " + childName + " but the parent node contains multiple children (" + node.ChildNodes.Count + ")");

                    if (!this.IsRegisteredClass(childName))
                        throw new InvalidDataException("Node " + node.Name + " specifies provider name " + childName + ", which must be registered but is not registered");
                    if (item != null)
                        throw new InvalidDataException("Node " + node.Name + " specifies provider name " + childName + ", but a parent node already specified to create " + item);
                    // create an object of the right type using the appropriate provider
                    item = this.NewInstance(childName, objectDescriptor.Type);
                    // parse any remaining properties in the object
                    item = this.parse(childNode, new ObjectDescriptor(item));
                    break;
                }
                // call the constructor if the item is non-null, since we now know that need to set a property on the element
                if (item == null)
                {
                    if (childNode.ChildNodes.Count == 0)
                    {
                        string childText = childNode.InnerText;
                        if (Object.Equals(objectDescriptor.Type, childText.GetType()))
                            item = childText;
                        else
                            item = new ConstantValue_Provider<string>(childNode.InnerText);
                        this.assertType(item, objectDescriptor.Type);
                        return item;
                    }
                    item = this.NewInstance(null, objectDescriptor.Type);
                }

                // Determine the child type that the parent wants, and parse the child
                Type childType = this.getPropertyType(item.GetType(), childName);
                childItem = this.parse(childNode, new ObjectDescriptor(childType));

                // invoke the setter
                PropertyInfo propertyInfo = this.getProperty(item.GetType(), childName);
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
                item = this.NewInstance(null, objectDescriptor.Type);

            //Logger.Message("done parsing " + node.Name + " as " + item);
            return item;
        }
    
        private void assertType(object item, Type type)
        {
            if (item == null || type == null)
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
            return (this.IsRegisterableClass(name) && this.providers.ContainsKey(name));
        }
        // creates a new instance using the provider registered for this name, or a new instance if no provider is registered
        public object NewInstance(string name, Type defaultType)
        {
            ValueConverter<Type, object> provider = this.defaultProvider;
            if (this.IsRegisterableClass(name))
            {
                if (this.providers.ContainsKey(name))
                    provider = this.providers[name];
            }
            object result = provider.ConvertValue(defaultType);
            this.assertType(result, defaultType);
            return result;
        }
        public void RegisterProvider(string name, ValueConverter<Type, object> initializer)
        {
            if (!this.IsRegisterableClass(name))
            {
                throw new ArgumentException("Type name '" + name + "' does not fit the naming conventions required to register as a class name");
            }
            if (this.IsRegisteredClass(name))
            {
                throw new InvalidOperationException("Type name '" + name + "' is already registered to '" + this.providers[name] + " and cannot be registered as " + initializer);
            }
            this.providers[name] = initializer;
        }
        public void RegisterClass(string name, object example)
        {
            this.RegisterProvider(name, new NewInstance_Provider(example));
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

        Dictionary<string, ValueConverter<Type, object>> providers = new Dictionary<string, ValueConverter<Type, object>>();
        ValueConverter<Type, object> defaultProvider = new NewInstance_Provider();
        string projectFileName = "Project.xml";
        
    }


    public class ObjectDescriptor
    {
        public ObjectDescriptor()
        {
        }
        public ObjectDescriptor(Type type)
        {
            this.Type = type;
        }
        public ObjectDescriptor(object item)
        {
            this.Item = item;
        }
        public Type Type { get; set; }
        public object Item
        {
            get
            {
                return this.item;
            }
            set
            {
                this.item = value;
                if (this.item != null)
                    this.type = this.item.GetType();
                else
                    this.type = null;
            }
        }

        private Type type;
        private object item;
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
