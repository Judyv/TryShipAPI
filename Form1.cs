using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceModel;
using TryShipAPI.StarShipShipAPI;

namespace TryShipAPI
{
    public partial class Form1 : Form
    {
        public SettingsManager settingsmanager;
        public ShipClient shippingclient;
        public StarShipRateAPI.DataTransactionsClient ratingclient;
        public Identity identity;
        public ClientAuthentication clientauthentication;
        public int CurrentLocationID = -1;

        public Form1()
        {
            InitializeComponent();
            lbLocationCode.Text = "";
            string datapath = Environment.ExpandEnvironmentVariables("%LocalAppData%") + "\\V-Technologies\\StarShip\\RateTool\\";
            if (System.IO.Directory.Exists(datapath) == false)
                System.IO.Directory.CreateDirectory(datapath);
            settingsmanager = new SettingsManager(datapath + "TryShipAPI.XML");
            edStarShipServer.Text = settingsmanager.Settings.StarShipServer;
            edSSUser.Text = settingsmanager.Settings.StarShipUser;
            edSSPassword.Text = settingsmanager.Settings.StarShipUser;
            edDevKey.Text = settingsmanager.Settings.DeveloperKey;

            if (settingsmanager.Settings.StarShipServer.Trim().Length > 0)
            {
                LoadLocations();
            }
                     
        }
        
        private void LoadLocations()
        {
            try
            {
                cbSSLocation.DisplayMember = "Name";
                BasicHttpBinding basicbinding = new BasicHttpBinding();
                ratingclient = new StarShipRateAPI.DataTransactionsClient(basicbinding, new EndpointAddress("http://" + settingsmanager.Settings.StarShipServer + ":3315/WebServicesServer/Data"));
                StarShipRateAPI.LoadLocationsRequest loadlocationsrequest = new StarShipRateAPI.LoadLocationsRequest
                {
                    Identity = new StarShipRateAPI.Identity
                    {
                        ApplicationName = "Sample",
                        ApplicationVersion = "1.0",
                        DeveloperKey = ""
                    },
                    Authentication = null
                };
                StarShipRateAPI.LoadLocationsResponse locationsresponse = new StarShipRateAPI.LoadLocationsResponse();
                locationsresponse = ratingclient.LoadLocations(loadlocationsrequest);
                for (int i = 0; i < locationsresponse.Locations.Length; i++)
                {
                    cbSSLocation.Items.Add(locationsresponse.Locations[i]);
                }
                
                if (settingsmanager.Settings.StarShipLocation.Trim().Length > 0)
                {
                    cbSSLocation.SelectedIndex = cbSSLocation.FindStringExact(settingsmanager.Settings.StarShipLocation);
                }
                else
                    cbSSLocation.SelectedIndex = 0; 
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error loading locations : " + ex.Message);
            }
            
        }

        private void initconnection()
        {
            BasicHttpBinding basicbinding = new BasicHttpBinding();
            shippingclient = new ShipClient(basicbinding, new EndpointAddress("http://" + settingsmanager.Settings.StarShipServer + ":3316/Ship"));
            identity = new Identity()
            {
                ApplicationName = "TryShipAPI",
                ApplicationVersion = "1.0",
                DeveloperKey = settingsmanager.Settings.DeveloperKey
            };
            clientauthentication = new ClientAuthentication()
            {
                UserID = settingsmanager.Settings.StarShipUser,
                Password = settingsmanager.Settings.StarShipPassword,
                LocationID = settingsmanager.Settings.StarShipLocationID
            };            
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            // SHIPSHIPMENT
            initconnection();
            try
            {
                ShipShipmentRequest sRequest = new ShipShipmentRequest();
                sRequest.Identity = identity;
                sRequest.Authentication = clientauthentication;
                sRequest.Params = new ShipParams()
                {
                    AssignSSCC = AssignSSCCNumbers.All,
                    DeliverBy = new DateTime(),
                    WritebackToSource = false
                };
                sRequest.Shipment = new Shipment();
                sRequest.Shipment.Recipient = new Recipient()
                {
                    Address = new Address
                    {
                        Name = "Aaron Fitz Electrical",
                        Address1 = "11403 45 St. South",
                        Address2 = "Suite 405",
                        City = "Chicago",
                        StateProvinceCode = "IL",
                        PostalCode = "606030776",
                        CountryCode = "US",
                        LocationType = LocationType.Business
                    },
                    Contact = new Contact
                    {
                        Name = "Misty Robinson",
                        Phone = "8605851400",
                        Email = "mistyr@gmail.com"
                    }
                };
                sRequest.Shipment.Sender = new Sender()
                {
                    Address = new Address
                    {
                        Name = "V-Technologies, LLC",
                        Address1 = "675 W Johnson Ave",
                        Address2 = "Suite 2000",
                        City = "Cheshire",
                        StateProvinceCode = "CT",
                        PostalCode = "06410",
                        CountryCode = "US",
                        LocationType = LocationType.Business
                    },
                    Contact = new Contact
                    {
                        Name = "James Mapes",
                        Phone = "8004624016",
                        Email = "jamesm@vtech.com"
                    },
                    AccountInfoList = new AccountInfo[1]
                };
                sRequest.Shipment.Sender.AccountInfoList[0] = new AccountInfo()
                {
                    AccountNumber = "510195400", // "Default" doesn't work....
                    CarrierInterfaceID = "FX",
                    SCAC = "FEDE"
                };

                sRequest.Shipment.Billing = new Billing()
                {
                    BillingType = StarShipShipAPI.BillingType.Prepaid,
                    BillingOptionID = InternationalBillingOptionType.Transportation,
                    BillingAccountNumber = "510195400",
                    BillDutiesTaxesType = BillDutiesTaxesType.Sender
                };

                sRequest.Shipment.ShipCarrier = new ShipCarrier()
                {
                    CarrierInterfaceID = "FX",
                    CarrierName = "FedEx",
                    SCAC = "FEDE",
                    ServiceGroup = ServiceGroup.Ground,
                    CarrierType = CarrierType.Parcel,
                    ServiceID = "GROUND",
                    ServiceName = "FedEx Ground®",
                    AccountNumber = "510195400"  // "Default" -- need account here
                };
                sRequest.Shipment.ShipDate = DateTime.Now;
                sRequest.Shipment.Packs = new Pack[1];
                sRequest.Shipment.Packs[0] = new Pack
                {
                    ActualWeight = new Weight
                    {
                        Value = 5.0M,
                        UOM = WeightUOMType.LB
                    },
                    Name = "Custom Box",
                    PackQty = 1,
                    PackagingType = PackagingTypeEnum.Box,
                    Dimensions = new Dimensions
                    {
                        Length = 10,
                        Height = 5,
                        Width = 5,
                        UOM = DimsUOMType.inch
                    }
                };
                ShipShipmentResponse sResponse = new ShipShipmentResponse();
                try
                {
                    sResponse = shippingclient.ShipShipment(sRequest);
                    if (sResponse.ResponseInfo.ResultCode.ToString() == "Failure")
                    {
                        System.Windows.Forms.MessageBox.Show("Error in ShipShipment : " + sResponse.ResponseInfo.Description.ToString());
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Success!");
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Error in ShipShipment : " + ex.Message);
                }
            }
            finally
            {
                shippingclient.Close();
            }

        }

        private void btCreate_Click(object sender, EventArgs e)
        {
            initconnection();
            try
            {
                try
                {
                    // CREATESHIPMENT
                    CreateShipmentRequest cRequest = new CreateShipmentRequest();
                    cRequest.Authentication = clientauthentication;
                    cRequest.Identity = identity;
                    cRequest.SaveShipment = true;   // for speed, don't save shipment to StarShip DB (for test/debug, it helps to save it and see what you're getting)

                    cRequest.SourceDocument = new SourceDocument();
                    cRequest.SourceDocument.Company = "Macola ES Sandbox";
                    cRequest.SourceDocument.SourceID = 22;
                    cRequest.SourceDocument.SourceName = "Macola ES";
                    cRequest.SourceDocument.DocumentKey = "85";
                    cRequest.SourceDocument.DocumentName = "Orders";
                    cRequest.SourceDocument.DocumentType = SourceDocumentType.Shipment;    // using a customization that makes Macola Orders document = Shipment type to allow packaging
                    cRequest.SourceDocument.Loaded = true;
                    // Need this to write back to source on ShipShipment
                    cRequest.SourceDocument.SourceAttributes = new SourceField[]
                    {
                        new SourceField{Name = "_WritebackSupported",Value = "True"}
                    };
                    cRequest.SourceDocument.HeaderFields = new SourceField[]
                    {
                        new SourceField{Name = "Order No.", Value = "85"},
                        new SourceField{Name = "PO Number", Value = "23434"},
                        new SourceField{Name = "Ship To Address Line 1", Value = "1786 Northwood Plaze"},
                        new SourceField{Name = "Ship To Address Line 2", Value = "Suite 203"},
                        new SourceField{Name = "Ship To Contact Name", Value = "Sally George"},
                        new SourceField{Name = "Ship To City", Value = "Atlanta"},
                        new SourceField{Name = "Ship To Name", Value = "Bike O'Rama"},
                        new SourceField{Name = "Ship To Country", Value = "United States of America"},
                        new SourceField{Name = "Cust No.", Value = "902"},
                        new SourceField{Name = "Cust E-mail", Value = ""},
                        new SourceField{Name = "Ship To Zip", Value = "30329"},
                        new SourceField{Name = "Ship To State", Value = "GA"},
                        new SourceField{Name = "Ship To Phone", Value = "789-987-6541"},
                        new SourceField{Name = "Ship Via", Value = "Air Freight"},
                        new SourceField{Name = "Terms", Value = "Net 30"}
                    };
                    cRequest.SourceDocument.InnerOrders = new Document[]
                    {
                    new Document 
                    {
                    DocumentKey = "85",
                    DocumentType = SourceDocumentType.Shipment,  
                    HeaderFields = new SourceField[]
                    {
                        new SourceField{Name = "Order No.", Value = "85"},
                        new SourceField{Name = "PO Number", Value = "23434"},
                        new SourceField{Name = "DocumentKey", Value = "85"},
                    },
                    // To access line items when adding packing, I mapped any unused field (here "Extra 1") to "Extra Key 1"
                    // and assigned unique number 0, 1, 2 (for reall application would be an internal key) this does two things:
                    //    1. prevents StarShip from rolling up lines with same item number and UOM  
                    //    2. allows me to access a specific line item within response when packing
                    LineItems = new SourceLineItem[]
                    {
                        new SourceLineItem
                        {
                            LineItemNumber = "1",
                            LineItemFields = new SourceField[]
                            {
                                new SourceField{Name = "Item Description", Value = "Cable for Brakes 1"},
                                new SourceField{Name = "Item No.", Value = "BCABLE"},
                                new SourceField{Name = "Item Sequential Number", Value = "1"},
                                new SourceField{Name = "Item Quantity ordered", Value = "2"},
                                new SourceField{Name = "Item UOM", Value = "FT"},
                                new SourceField{Name = "Item Unit Price", Value = "5"},
                                new SourceField{Name = "Item Weight", Value = "4"},
                                new SourceField{Name = "Item Quantity Shipped|Returned", Value = "2"},
                                new SourceField{Name = "Extra 1", Value = "0"}     
                            } 
                        },
                        new SourceLineItem
                        {
                            LineItemNumber = "2",
                            LineItemFields = new SourceField[]
                            {
                                new SourceField{Name = "Item Description", Value = "Break and Cable Assembly"},
                                new SourceField{Name = "Item No.", Value = "BCABASSY"},
                                new SourceField{Name = "Item Sequential Number", Value = "2"},
                                new SourceField{Name = "Item Quantity ordered", Value = "2"},
                                new SourceField{Name = "Item UOM", Value = "EA"},
                                new SourceField{Name = "Item Unit Price", Value = "160"},
                                new SourceField{Name = "Item Weight", Value = "5"},
                                new SourceField{Name = "Item Quantity Shipped|Returned", Value = "2"},
                                new SourceField{Name = "Extra 1", Value = "1"}      
                            }
                        },
                        new SourceLineItem
                        {
                            LineItemNumber = "3",
                            LineItemFields = new SourceField[]
                            {
                                new SourceField{Name = "Item Description", Value = "Gloves Womens Small"},
                                new SourceField{Name = "Item No.", Value = "GLOVESSW"},
                                new SourceField{Name = "Item Sequential Number", Value = "3"},
                                new SourceField{Name = "Item Quantity ordered", Value = "3"},
                                new SourceField{Name = "Item UOM", Value = "EA"},
                                new SourceField{Name = "Item Unit Price", Value = "5"},
                                new SourceField{Name = "Item Weight", Value = "1"},
                                new SourceField{Name = "Item Quantity Shipped|Returned", Value = "3"},
                                new SourceField{Name = "Extra 1", Value = "2"}      
                            }
                        }
                    }
                    }
                    // you can add additional documents here!
                };

                    CreateShipmentResponse Response = new CreateShipmentResponse();
                    try
                    {
                        Response.Shipment = new Shipment();
                        Response = shippingclient.CreateShipment(cRequest);
                        if (Response.ResponseInfo.ResultCode.ToString() == "Failure")
                        {
                            throw new Exception("CreateShipment returned failure " + Response.ResponseInfo.Description.ToString());
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("CreateShipment returned Success!");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error calling CreateShipment : " + ex.Message);
                    }
                    // now we can add packaging in returned shipment and call ShipShipment
                    Response.Shipment.Packs = new Pack[]
                {
                    // We are going to create thre packages containing the three line items as follows
                    // Note: the value I sent in for field "Extra 1" gets returned as "ExtraKey" due to my customized interface mapping
                    //
                    // Pkg   Item Number   ExtraKey    Qty
                    // 1     BCABLE           0        2
                    // 1     GLOVESSSW        2        3
                    // 2     BCABASSY         1        1
                    // 3     BCABASSY         1        1                                     
                    new Pack
                    {

                        ActualWeight = new Weight
                        {
                            UOM = WeightUOMType.LB,
                            Value = 10.5M
                        },
                        Dimensions = new Dimensions
                        {
                            Length = 10,
                            Height = 5,
                            Width = 10
                        },
                        Name = "Big Box",
                        PackagingType = PackagingTypeEnum.Box,
                        PackQty = 1,
                        DocumentKey = "85",
                        PalletID = -1,
                        LineItems = new LineItem[]
                        {
                            new LineItem
                            {
                                ItemID = Response.Shipment.LineItems.FirstOrDefault(s => s.ExtraKey.Equals("0")).ItemID,
                                OrderNumber = "85",
                                ShipQty = 2
                            },
                            new LineItem
                            {
                                ItemID = Response.Shipment.LineItems.FirstOrDefault(s => s.ExtraKey.Equals("2")).ItemID,
                                OrderNumber = "85",
                                ShipQty = 3
                            }
                        }
                    },
                    new Pack
                    {

                        ActualWeight = new Weight
                        {
                            UOM = WeightUOMType.LB,
                            Value = 5.5M
                        },
                        Dimensions = new Dimensions
                        {
                            Length = 12,
                            Height = 6,
                            Width = 11
                        },
                        Name = "Medium Carton",
                        PackagingType = PackagingTypeEnum.Box,
                        PackQty = 1,
                        DocumentKey = "85",
                        PalletID = -1,
                        LineItems = new LineItem[]
                        {
                            new LineItem
                            {
                                ItemID = Response.Shipment.LineItems.FirstOrDefault(s => s.ExtraKey.Equals("1")).ItemID,
                                OrderNumber = "85",
                                ShipQty = 1
                            }                            
                        }
                    },
                    new Pack
                    {

                        ActualWeight = new Weight
                        {
                            UOM = WeightUOMType.LB,
                            Value = 5.5M
                        },
                        Dimensions = new Dimensions
                        {
                            Length = 12,
                            Height = 6,
                            Width = 7
                        },
                        Name = "Medium Carton",
                        PackagingType = PackagingTypeEnum.Box,
                        PackQty = 1,
                        DocumentKey = "85",
                        PalletID = -1,
                        LineItems = new LineItem[]
                        {
                            new LineItem
                            {
                                ItemID = Response.Shipment.LineItems.FirstOrDefault(s => s.ExtraKey.Equals("1")).ItemID,
                                OrderNumber = "85",
                                ShipQty = 1
                            }                            
                        }
                    }
                };

                    // SHIPSHIPMENT
                    ShipShipmentRequest sRequest = new ShipShipmentRequest();
                    sRequest.Identity = identity;
                    sRequest.Authentication = clientauthentication;
                    sRequest.Params = new ShipParams()
                    {
                        AssignSSCC = AssignSSCCNumbers.All,
                        DeliverBy = new DateTime(),
                        WritebackToSource = true
                    };
                    sRequest.Shipment = Response.Shipment;
                    sRequest.Shipment.FSIDocInfo.WritebackFreightCharges = true;                                    

                    ShipShipmentResponse sResponse = new ShipShipmentResponse();
                    try
                    {
                        sResponse = shippingclient.ShipShipment(sRequest);
                        if (sResponse.ResponseInfo.ResultCode.ToString() == "Failure")
                        {
                            System.Windows.Forms.MessageBox.Show("Error in ShipShipment : " + sResponse.ResponseInfo.Description.ToString());
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("ShipShipment Successful!");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show("Error in ShipShipment : " + ex.Message);
                    }

                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Error : " + ex.Message);
                }
            }
            finally
            {
                shippingclient.Close();
            }
        }

        private void btShipSourceDocument_Click(object sender, EventArgs e)
        {
            initconnection();
            try
            {
                try
                {
                    // SHIPSOURCEDOCUMENT 
                    ShipSourceDocumentRequest cRequest = new ShipSourceDocumentRequest();
                    cRequest.Identity = identity;
                    cRequest.Authentication = clientauthentication;
                    cRequest.Params = new ShipParams()
                    {
                        AssignSSCC = AssignSSCCNumbers.All,
                        DeliverBy = new DateTime(),
                        WritebackToSource = true
                    };
                    cRequest.Document = new SourceDocument();
                    cRequest.Document.Company = "Macola ES Sandbox";
                    cRequest.Document.SourceID = 22;
                    cRequest.Document.SourceName = "Macola ES";
                    cRequest.Document.DocumentKey = "85";
                    cRequest.Document.DocumentName = "Orders";
                    cRequest.Document.DocumentType = SourceDocumentType.Shipment;    // using a customization that makes Macola Orders document = Shipment type to allow packaging
                    cRequest.Document.Loaded = true;
                    cRequest.Document.SourceAttributes = new SourceField[]
                    {
                        new SourceField{Name = "_WritebackSupported",Value = "True"}                        
                    };
                    cRequest.Document.HeaderFields = new SourceField[]
                    {
                        new SourceField{Name = "Order No.", Value = "85"},
                        new SourceField{Name = "PO Number", Value = "23434"},
                        new SourceField{Name = "Ship To Address Line 1", Value = "1786 Northwood Plaze"},
                        new SourceField{Name = "Ship To Address Line 2", Value = "Suite 203"},
                        new SourceField{Name = "Ship To Contact Name", Value = "Sally George"},
                        new SourceField{Name = "Ship To City", Value = "Atlanta"},
                        new SourceField{Name = "Ship To Name", Value = "Bike O'Rama"},
                        new SourceField{Name = "Ship To Country", Value = "United States of America"},
                        new SourceField{Name = "Cust No.", Value = "902"},
                        new SourceField{Name = "Cust E-mail", Value = ""},
                        new SourceField{Name = "Ship To Zip", Value = "30329"},
                        new SourceField{Name = "Ship To State", Value = "GA"},
                        new SourceField{Name = "Ship To Phone", Value = "789-987-6541"},
                        new SourceField{Name = "Ship Via", Value = "Air Freight"},
                        new SourceField{Name = "Terms", Value = "Net 30"}
                    };
                    // Set up mapping to some unused Macola fields as follows:
                    // StarShip Field           Source Field
                    // LineItem > Extra Key 1   Extra 1
                    // Packaging > Packaging    Extra 2
                    // Packaging > Length       Extra 3
                    // Packaging > Width        Extra 4
                    // Packaging > Height       Extra 5
                    // Packaging > TotWeight    Extra 6
                    // Packaging > Type         fixed value "Box"
                    // Packaging > Quantity     fixed value "1"
                    // Packaging > Dims UOM     fixed vlaue "in."
                    // Packaging > Weight UOM   fixed value "lbs"
                    cRequest.Document.InnerOrders = new Document[]
                    {
                        new Document 
                        {
                            DocumentKey = "85",
                            DocumentType = SourceDocumentType.Shipment, 
                            HeaderFields = new SourceField[]
                            {
                                new SourceField{Name = "Order No.", Value = "85"},
                                new SourceField{Name = "PO Number", Value = "23434"},
                                new SourceField{Name = "DocumentKey", Value = "85"},
                            },
                            // by default, Staship matches up items in package to lines in order by order number, item number, and UOM. 
                            // I mapped an unused Macola field "Extra 1" to StarShip's "Extra Key 1" and assigned a unique numeric value to it.
                            // This will make my packing precise even if multiple lines in the same order share the same item number and UOM.
                            LineItems = new SourceLineItem[]
                            {
                            new SourceLineItem
                            {
                                LineItemNumber = "1",
                                LineItemFields = new SourceField[]
                                {
                                    new SourceField{Name = "Item Description", Value = "Cable for Brakes 1"},
                                    new SourceField{Name = "Item No.", Value = "BCABLE"},
                                    new SourceField{Name = "Item Sequential Number", Value = "1"},
                                    new SourceField{Name = "Item Quantity ordered", Value = "2"},
                                    new SourceField{Name = "Item UOM", Value = "FT"},
                                    new SourceField{Name = "Item Unit Price", Value = "5"},
                                    new SourceField{Name = "Item Weight", Value = "4"},
                                    new SourceField{Name = "Item Quantity Shipped|Returned", Value = "2"},
                                    new SourceField{Name = "Extra 1", Value = "0"}     
                                } 
                            },
                            new SourceLineItem
                            {
                                LineItemNumber = "2",
                                LineItemFields = new SourceField[]
                                {
                                    new SourceField{Name = "Item Description", Value = "Break and Cable Assembly"},
                                    new SourceField{Name = "Item No.", Value = "BCABASSY"},
                                    new SourceField{Name = "Item Sequential Number", Value = "2"},
                                    new SourceField{Name = "Item Quantity ordered", Value = "2"},
                                    new SourceField{Name = "Item UOM", Value = "EA"},
                                    new SourceField{Name = "Item Unit Price", Value = "160"},
                                    new SourceField{Name = "Item Weight", Value = "5"},
                                    new SourceField{Name = "Item Quantity Shipped|Returned", Value = "2"},
                                    new SourceField{Name = "Extra 1", Value = "1"}      
                                }
                            },
                            new SourceLineItem
                            {
                                LineItemNumber = "3",
                                LineItemFields = new SourceField[]
                                {
                                    new SourceField{Name = "Item Description", Value = "Gloves Womens Small"},
                                    new SourceField{Name = "Item No.", Value = "GLOVESSW"},
                                    new SourceField{Name = "Item Sequential Number", Value = "3"},
                                    new SourceField{Name = "Item Quantity ordered", Value = "3"},
                                    new SourceField{Name = "Item UOM", Value = "EA"},
                                    new SourceField{Name = "Item Unit Price", Value = "5"},
                                    new SourceField{Name = "Item Weight", Value = "1"},
                                    new SourceField{Name = "Item Quantity Shipped|Returned", Value = "3"},
                                    new SourceField{Name = "Extra 1", Value = "2"}      
                                }
                            }
                            }                                               
                        }
                        // you can add additional documents here!
                    };

                    // We are going to create three packages containing the three line items as follows
                    // Note: the value I sent in for field "Extra 1" gets returned as "ExtraKey" due to my customized interface mapping
                    //
                    // Pkg   Item Number   ExtraKey    Qty
                    // 1     BCABLE           0        2
                    // 1     GLOVESSW         2        3
                    // 2     BCABASSY         1        1
                    // 3     BCABASSY         1        1                                     
                    cRequest.Document.Packages = new SourcePackage[]
                    {
                        new SourcePackage
                        {
                            PackageID = "1",
                            PackageFields = new SourceField[]
                            {
                                new SourceField{Name = "Extra 2", Value = "Big Box"},
                                new SourceField{Name = "Extra 3", Value = "10"},
                                new SourceField{Name = "Extra 4", Value = "10"},
                                new SourceField{Name = "Extra 5", Value = "5"},
                                new SourceField{Name = "Extra 6", Value = "10.5"},
                            },
                            Content = new PackageContent[]
                            {
                                new PackageContent
                                {
                                    ContentFields = new SourceField[]
                                    {
                                        new SourceField{Name = "Extra 1", Value = "0"},                                   
                                        new SourceField{Name = "Extra 7", Value = "85"},                                   
                                        new SourceField{Name = "Extra 8", Value = "BCABLE"},                                   
                                        new SourceField{Name = "Extra 9", Value = "2"},                                   
                                        new SourceField{Name = "Extra 10", Value = "FT"},                                   
                                    }
                                },
                                new PackageContent
                                {
                                    ContentFields = new SourceField[]
                                    {
                                        new SourceField{Name = "Extra 1", Value = "2"},                                   
                                        new SourceField{Name = "Extra 7", Value = "85"},                                   
                                        new SourceField{Name = "Extra 8", Value = "GLOVESSW"},                                   
                                        new SourceField{Name = "Extra 9", Value = "3"},                                   
                                        new SourceField{Name = "Extra 10", Value = "EA"},                                   
                                    }
                                }
                            }
                        },
                        new SourcePackage
                        {
                            PackageID = "2",
                            PackageFields = new SourceField[]
                            {
                                new SourceField{Name = "Extra 2", Value = "Medium Carton"},
                                new SourceField{Name = "Extra 3", Value = "12"},
                                new SourceField{Name = "Extra 4", Value = "11"},
                                new SourceField{Name = "Extra 5", Value = "6"},
                                new SourceField{Name = "Extra 6", Value = "5.5"},
                            },
                            Content = new PackageContent[]
                            {
                                new PackageContent
                                {
                                    ContentFields = new SourceField[]
                                    {
                                        new SourceField{Name = "Extra 1", Value = "1"},                                   
                                        new SourceField{Name = "Extra 7", Value = "85"},                                   
                                        new SourceField{Name = "Extra 8", Value = "BCABASSY"},                                   
                                        new SourceField{Name = "Extra 9", Value = "1"},                                   
                                        new SourceField{Name = "Extra 10", Value = "EA"},                                   
                                    }
                                }
                            }
                        },
                        new SourcePackage
                        {
                            PackageID = "2",
                            PackageFields = new SourceField[]
                            {  
                                new SourceField{Name = "Extra 2", Value = "Medium Carton"},
                                new SourceField{Name = "Extra 3", Value = "12"},
                                new SourceField{Name = "Extra 4", Value = "11"},
                                new SourceField{Name = "Extra 5", Value = "6"},
                                new SourceField{Name = "Extra 6", Value = "5.5"},
                            },
                            Content = new PackageContent[]
                            {
                                new PackageContent
                                {
                                    ContentFields = new SourceField[]
                                    {
                                        new SourceField{Name = "Extra 1", Value = "1"},                                   
                                        new SourceField{Name = "Extra 7", Value = "85"},                                   
                                        new SourceField{Name = "Extra 8", Value = "BCABASSY"},                                   
                                        new SourceField{Name = "Extra 9", Value = "1"},                                   
                                        new SourceField{Name = "Extra 10", Value = "EA"},                                   
                                    }
                                }
                             }
                        }
                    };

                    ShipShipmentResponse sResponse = new ShipShipmentResponse();
                    try
                    {
                        sResponse = shippingclient.ShipSourceDocument(cRequest);
                        if (sResponse.ResponseInfo.ResultCode.ToString() == "Failure")
                        {
                            System.Windows.Forms.MessageBox.Show("Error in ShipShipment : " + sResponse.ResponseInfo.Description.ToString());
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("ShipShipment Successful!");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show("Error in ShipShipment : " + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Error : " + ex.Message);
                }
            }
            finally
            {
                shippingclient.Close();
            }
        }

        private void cbSSLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            settingsmanager.Settings.StarShipLocation = cbSSLocation.Text;
            settingsmanager.Settings.StarShipLocationID = (cbSSLocation.SelectedItem as StarShipRateAPI.Location).Code;
            lbLocationCode.Text = settingsmanager.Settings.StarShipLocationID.ToString();        
        }


        private void edStarShipServer_Leave(object sender, EventArgs e)
        {
            if (settingsmanager.Settings.StarShipServer.Trim().ToUpper() != edStarShipServer.Text.Trim().ToUpper())
            {
                settingsmanager.Settings.StarShipServer = edStarShipServer.Text.Trim();
                LoadLocations();
            };
            checkvalid();
        }

        private void edSSUser_TextChanged(object sender, EventArgs e)
        {
            settingsmanager.Settings.StarShipUser = edSSUser.Text;
            checkvalid();
        }

        private void edSSPassword_TextChanged(object sender, EventArgs e)
        {
            settingsmanager.Settings.StarShipPassword = edSSPassword.Text;
            checkvalid();
        }

        private void edDevKey_TextChanged(object sender, EventArgs e)
        {
            settingsmanager.Settings.DeveloperKey = edDevKey.Text;
            checkvalid();
        }

        private void checkvalid()
        {
            if (settingsmanager.SettingsAreValid())
            {
                btCreate.Enabled = true;
                btShipShipment.Enabled = true;
                btShipSourceDocument.Enabled = true;
            }
            else
            {
                btCreate.Enabled = false;
                btShipShipment.Enabled = false;
                btShipSourceDocument.Enabled = false;
            };
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            settingsmanager.StoreSettings();
        }

        private void edStarShipServer_TextChanged(object sender, EventArgs e)
        {
            if (edStarShipServer.Text != settingsmanager.Settings.StarShipServer)
            {
                cbSSLocation.Items.Clear();
                cbSSLocation.Text = "";
                lbLocationCode.Text = "";
            }
        }
    }
}
