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
            ProjectDTO projectDTO = (ProjectDTO)this.FileToObject(filePath);
            Project project = projectDTO.GetValue();
            String directoryPath = Directory.GetParent(filePath).FullName;
            project.source.GetValue().location = new FileLocation(directoryPath);
            project.parser = this;
            return project;
        }

        public void SaveProject(string filePath, Project project)
        {
            ProjectDTO dto = new ProjectDTO(project);
            string content = this.objectToString(dto, dto.GetType());
            File.WriteAllText(filePath + "\\" + this.projectFileName, content);
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

        private string objectToString(object item, Type expectedType)
        {
            string newline = Environment.NewLine;
            if (item == null)
                return null;
            if (this.itemSources.ContainsKey(item))
            {
                // This item was parsed from a file before; let's check whether it's changed
                XmlNode previousNode = this.itemSources[item];
                object reparsed = this.parse(previousNode);
                if (reparsed.Equals(item))
                {
                    // the object hasn't changed since we last parsed it, so we don't have to change its formatting and we'll keep the user's existing formatting
                    return previousNode.OuterXml;
                }
            }
            if (item is string)
            {
                return item.ToString();
            }
            ValueProvider<string> provider = item as ValueProvider<string>;
            if (provider != null)
            {
                return provider.GetValue();
            }
            // Here we have to regenerate some text from this object
            string xml = "";
            //if (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition().Equals(typeof(List<>).GetGenericTypeDefinition()))
            if (item is List<ProjectDescriptorDTO>)
            {
                // fill in a bunch of content for the list
                Type propertyType = this.getPropertyType(item.GetType(), "item");
                foreach (object child in (IEnumerable<object>)item)
                {
                    string childContent = this.objectToString(child, propertyType);
                    xml += newline + "<item>" + this.Indent(childContent) + "</item>" + newline;
                }
            }
            else
            {
                // fill in a bunch of content for an object
                bool newlined = false;
                foreach (PropertyInfo propertyInfo in item.GetType().GetProperties())
                {
                    // get the value of the property
                    object[] parameters = new object[] { };
                    MethodInfo getter = propertyInfo.GetMethod;
                    if (getter.GetParameters().Count() == 0)
                    {
                        object subItem = getter.Invoke(item, parameters);
                        string childText = this.objectToString(subItem, getter.ReturnType);
                        if (childText != null)
                        {
                            // create an xml string from the child object
                            string propertyAlias = propertyInfo.Name;
                            if (!newlined)
                            {
                                xml += newline;
                                newlined = true;
                            }
                            xml += "<" + propertyAlias + ">" + this.Indent(childText) + "</" + propertyAlias + ">" + newline;
                        }
                    }
                }
            }


            Type correctType = item.GetType();
            foreach (string name in this.providers.Keys)
            {
                object providedObject = this.providers[name].ConvertValue(expectedType);
                if (providedObject.GetType().Equals(item.GetType()))
                {
                    if (xml.Length > 0)
                    {
                        xml = newline + "<" + name + ">" + this.Indent(xml) + "</" + name + ">" + newline;
                    }
                    else
                    {
                        xml = "<" + name + "/>";
                    }
                    break;
                }
            }
            return xml;
        }
   
        public string Indent(string xml)
        {
            string result = "";
            char separator = '\n';
            char[] separators = new char[] { separator };
            string[] lines = xml.Split(separators);
            if (lines.Count() == 1)
                return xml;
            bool first = true;
            foreach (string line in lines)
            {
                if (!first)
                    result += separator;
                if (line.Count() > 0)
                    result += "  " + line;
                first = false;
            }
            return result;
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
                if (childNode.Name == "#comment")
                    continue;
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
        public string ReadFile(String fileName)
        {
            StreamReader reader = new StreamReader(fileName);
            string result = reader.ReadToEnd();
            reader.Close();
            return result;
        }

        Dictionary<string, ValueConverter<Type, object>> providers = new Dictionary<string, ValueConverter<Type, object>>();
        ValueConverter<Type, object> defaultProvider = new NewInstance_Provider();
        string projectFileName = "Project.xml";

        Dictionary<object, XmlNode> itemSources = new Dictionary<object,XmlNode>(); // gives the XmlNode from which each parsed object was parsed

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
        public ParseableList()
        {

        }
        public ParseableList(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                this.Add(item);
            }
        }
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
