using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using System.Collections.Generic;

// The XmlRootAttribute allows you to set an alternate name 
// for the XML element and its namespace. By 
// default, the XmlSerializer uses the class name. The attribute 
// also allows you to set the XML namespace for the element. Lastly,
// the attribute sets the IsNullable property, which specifies whether 
// the xsi:null attribute appears if the class instance is set to 
// a null reference.
[XmlRootAttribute("StarShipSettings", 
IsNullable = false)]
public class StarShipSettings
{
    public string StarShipServer = "";
    public string StarShipUser = "";
    public string StarShipLocation = "";
    public string StarShipPassword = "";
    public string DeveloperKey = "";
    public int StarShipLocationID = -1;
    
}


public class SettingsManager
{
    public StarShipSettings Settings;
    public string filename;
    public bool SSConnected;
    public bool SettingsValidated;
    
    public void StoreSettings()
    {
        Settings.StarShipPassword = encode(Settings.StarShipPassword);
        // Creates an instance of the XmlSerializer class;
        // specifies the type of object to serialize.
        XmlSerializer serializer = new XmlSerializer(typeof(StarShipSettings));
        TextWriter writer = new StreamWriter(filename);
        // Serializes the purchase order, and closes the TextWriter.
        serializer.Serialize(writer, Settings);
        writer.Close();
    }

    public void LoadSettings()
    {
        try
        {
            // if file does not exist... just create new settings.
            if (File.Exists(filename) == false)
            {
                Settings = new StarShipSettings();
            }
            else
            {

                // Creates an instance of the XmlSerializer class;
                // specifies the type of object to be deserialized.
                XmlSerializer serializer = new XmlSerializer(typeof(StarShipSettings));
                // If the XML document has been altered with unknown 
                // nodes or attributes, handles them with the 
                // UnknownNode and UnknownAttribute events.
                serializer.UnknownNode += new
                XmlNodeEventHandler(serializer_UnknownNode);
                serializer.UnknownAttribute += new
                XmlAttributeEventHandler(serializer_UnknownAttribute);

                // A FileStream is needed to read the XML document.
                FileStream fs = new FileStream(filename, FileMode.Open);
                // Declares an object variable of the type to be deserialized.
                //DynRateToolSettings po;
                // Uses the Deserialize method to restore the object's state 
                // with data from the XML document. */
                Settings = (StarShipSettings)serializer.Deserialize(fs);
                Settings.StarShipPassword = decode(Settings.StarShipPassword);
                fs.Close();

            }
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show("Error loading settings : " + ex.Message);
        }
        
    }

    protected void serializer_UnknownNode
    (object sender, XmlNodeEventArgs e)
    {
        Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
    }

    protected void serializer_UnknownAttribute
    (object sender, XmlAttributeEventArgs e)
    {
        System.Xml.XmlAttribute attr = e.Attr;
        Console.WriteLine("Unknown attribute " +
        attr.Name + "='" + attr.Value + "'");
    }

    public SettingsManager(string settingfilename)
    {
        filename = settingfilename;
        SSConnected = false;
        SettingsValidated = false;
        
        LoadSettings();
    }


    static public string encode(string toEncode)
    {
        byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
        string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
        return returnValue;
    }

    static public string decode(string encodedData)
    {
        try
        {
        byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
        string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
        return returnValue;
        }
        catch
        {
            return encodedData;
        }
    }

    
    public bool SettingsAreValid()
    {
        bool tmpValid = true;
        if (Settings.StarShipServer.Trim().Length == 0)
            tmpValid = false;
        else if (Settings.StarShipUser.Trim().Length == 0)
            tmpValid = false;
        else if (Settings.StarShipLocation.Trim().Length == 0)
            tmpValid = false;       
        else if (Settings.StarShipPassword.Trim().Length == 0)
            tmpValid = false;
        else if (Settings.DeveloperKey.Trim().Length == 0)
            tmpValid = false;
        else if (Settings.StarShipLocationID < 0)
            tmpValid = false;
        
        return tmpValid;
    }
}