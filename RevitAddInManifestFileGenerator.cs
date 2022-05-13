// Copyright 2021 kkozlov

using System;
using System.Collections.Generic;
using System.Reflection;
using IO = System.IO;
using Xml = System.Xml.Linq;

namespace RevitUtils {
  enum AddInElementUsage {
    Required, Optional
  }
  
  [AttributeUsage(AttributeTargets.Property)]
  class AddInElementAttribute : Attribute {
    public AddInElementAttribute(AddInElementUsage usage) {
      _usage = usage;
    }

    public AddInElementUsage GetUsage() { return _usage; }

    AddInElementUsage _usage;
  }
      
  class AddInElements {
    [AddInElement(AddInElementUsage.Required)]
    public String Type { get; set; }
    [AddInElement(AddInElementUsage.Required)]
    public String Assembly { get; set; }
    [AddInElement(AddInElementUsage.Required)]
    public String AddInId { get; set; }
    [AddInElement(AddInElementUsage.Required)]
    public String FullClassName { get; set; }
    [AddInElement(AddInElementUsage.Required)]
    public String VendorId { get; set; }
    public String VendorDescription { get; set; }
    [AddInElement(AddInElementUsage.Required)]
    public String Name { get; set; }
    public String Description { get; set; }
    public String VisibilityMode { get; set; }
    public String Discipline { get; set; }
    public String AvailabilityClassName { get; set; }
    public String LargeImage { get; set; }
    public String SmallImage { get; set; }
    public String LongDescription { get; set; }
    public String ToolTipImage { get; set; }
    public String LanguageType { get; set; }
    public String AllowLoadIntoExistingSession { get; set; }
  }
  
  class Program {
    static void Main() {
      AddInElements data = PromptForData();
      GenerateManifestFile(data);
    }

    static AddInElements PromptForData() {
      AddInElements data = new AddInElements {
        AddInId = (Guid.NewGuid()).ToString()
      };
      Type type = data.GetType();
      PropertyInfo[] props = type.GetProperties();
      foreach (PropertyInfo prop in props) {
        String value = prop.GetValue(data) as String;
        if (null == value) {
          while (true) {
            Console.WriteLine("Please enter the value of {0} or q to exit",
                              prop.Name);
            value = Console.ReadLine();
            if (("q" == value || String.IsNullOrEmpty(value))
                && IsValueRequired(prop)) {
              Console.WriteLine("THE VALUE IS REQUIRED.");
              continue;
            } else if ("q" == value) {
              return data;
            } else if (value.Length > 0) {
              prop.SetValue(data, value);
              break;
            }
          }
        }
      }
      return data;
    }

    static Boolean IsValueRequired(PropertyInfo propInfo) {
      AddInElementAttribute addInAttr = Attribute.GetCustomAttribute(
          propInfo, typeof(AddInElementAttribute), false)
          as AddInElementAttribute;
      Boolean isRequired = false;
      if (addInAttr != null) {
        isRequired = AddInElementUsage.Required == addInAttr.GetUsage();
      }
      return isRequired;
    }

    static void GenerateManifestFile(AddInElements data) {
      Xml.XElement addin = new Xml.XElement(
          "AddIn", new Xml.XAttribute("Type", data.Type));
      Type type = data.GetType();
      PropertyInfo[] props = type.GetProperties();
      foreach (PropertyInfo prop in props) {
        String value = prop.GetValue(data) as String;
        if (!String.IsNullOrEmpty(value)) {
          addin.SetElementValue(prop.Name, value);
        }
      }
      Xml.XElement xmlTree = new Xml.XElement("RevitAddIns", addin);
      IO.StreamWriter sw = IO.File.CreateText(
          IO.Path.GetFileName(data.Assembly)
          .Replace(IO.Path.GetExtension(data.Assembly), ".addin"));
      sw.Write(xmlTree);
      sw.Close();
      Console.WriteLine(xmlTree);
    }
  }
}
