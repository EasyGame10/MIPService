using Dapper;
using MIPService.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace MIPService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private HttpClient client;
        public readonly AppSettings _appSettings;
        public IConfiguration _config;

        public Worker(ILogger<Worker> logger, AppSettings appSettings, IConfiguration config)
        {
            _logger = logger;
            _appSettings = appSettings;
            _config = config;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            client = new HttpClient();
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            client.Dispose();
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string username = _appSettings.username;
                string password = _appSettings.password;
                string zoho_base_url = _appSettings.zoho_base_client;
                string ZohoPost = _appSettings.Zoho;
                int timer = _appSettings.timer;

                try
                {
                    string refreshtoken = File.ReadAllText(@"C:\Token\MIPRefTok.txt");

                    if (refreshtoken == string.Empty)
                    {
                        var token = await GetAuthToken();
                        refreshtoken = token.refresh_token;
                    }

                    var accesstoken = await GetRefreshToken(refreshtoken);

                    if (accesstoken.access_token == null)
                    {
                        Log.Information("No accesstoken available" + accesstoken.error);
                        await Task.Delay(timer, stoppingToken);
                    }

                    var zohoclient = new RestClient();
                    var zohopar1 = new RestRequest(zoho_base_url + "search?criteria=((Comp_Code:equals:DRET)and(Stage:equals:Policy%20fulfillment)and(product:starts_with:4))");
                    zohopar1.AddHeader("Authorization", "Zoho-oauthtoken " + accesstoken.access_token);
                    var res1 = await zohoclient.ExecuteAsync(zohopar1);

                    var res = new List<string>();
                    if (res1.Content != "")
                    {
                        res.Add(res1.Content);
                    }

                    if (!res1.IsSuccessful)
                    {
                        Log.Information(res1.Content + " " + res1.ErrorMessage + " " + res1.ResponseStatus + " " + res1.StatusCode + "\n");
                        await Task.Delay(timer, stoppingToken);
                    }
                    var deserializedResults = new List<ZohoResponse>();

                    foreach (var responseContent in res)
                    {
                        try
                        {
                            var deserializedResponse = JsonConvert.DeserializeObject<ZohoResponse>(responseContent);
                            deserializedResults.Add(deserializedResponse);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Error during deserialization: " + ex.Message);
                            Log.Error("Response content: " + responseContent);
                            await Task.Delay(timer, stoppingToken);
                        }
                    }

                    foreach (var resultconv in deserializedResults)
                    {
                        if (resultconv.status == "error")
                        {
                            Log.Information(resultconv.message + " " + resultconv.code);
                            await Task.Delay(timer, stoppingToken);
                        }

                        if (resultconv.Data == null)
                        {
                            Log.Information("No records to post!");
                            await Task.Delay(timer, stoppingToken);
                        }


                        for (int i = 0; i < resultconv.Data.Length; i++)
                        {
                            using (SqlConnection con = new SqlConnection(_config.GetConnectionString("MIP")))
                            {
                                string data1 = string.Empty;
                                string table = string.Empty;
                                int dbInsert;
                                var exists = 0;
                                if (exists/*.Count()*/ == 0)
                                {
                                    Log.Information("There are " + resultconv.Data.Length + " record/s from zoho to post!");

                                    Log.Logger.Information(JsonConvert.SerializeObject(resultconv.Data[i]));
                                   
                                  
                                    var zocli = new RestClient(_appSettings.mip_base_client + "search?criteria=Name:equals:" + resultconv.Data[0].MIP.name);
                                    var zopa = new RestRequest();
                                    zopa.AddHeader("Authorization", "Zoho-oauthtoken " + accesstoken.access_token);

                                    var resGet = await zocli.GetAsync(zopa);

                                    var jsonResponse = resGet.Content;

                                    if (jsonResponse == "")
                                    {
                                      
                                        return;
                                    }
                                  

                                    JObject json = JObject.Parse(jsonResponse);

                                    // var mipRes = JsonConvert.DeserializeObject<MIPResponse>(jsonResponse);



                                    MemberDets mip = new MemberDets();


                                    mip.Xref = resultconv.Data[i].policynumber;
                                    mip.Title = resultconv.Data[i].title;
                                    mip.FirstName = resultconv.Data[i].firstname;
                                    mip.Surname = resultconv.Data[i].surname;
                                    if (resultconv.Data[i].id_type == "SA")
                                    {
                                        mip.IDNumber = resultconv.Data[i].id_number;
                                        mip.PassportNumber = "";
                                    }
                                    else
                                    {
                                        mip.PassportNumber = resultconv.Data[i].id_number;
                                        mip.IDNumber = "";
                                    }
                                    mip.DateOfBirth = resultconv.Data[i].date_of_birth;
                                    mip.Gender = resultconv.Data[i].gender[0].ToString();//change to first letter
                                    mip.Initials = $"{mip.FirstName[0]}.{mip.Surname[0]}.";
                                    mip.EmployerName = resultconv.Data[i].Comp_Code;
                                    
                                    mip.StatusDescription = resultconv.Data[i].mip_status; 
                                    mip.ResignationDate = "";


                                    mip.PostalAddress1 = resultconv.Data[i].postal_address_line_1;
                                    mip.PostalAddress2 = resultconv.Data[i].postal_address_line_2;
                                    mip.PostalAddress3 = resultconv.Data[i].suburb;
                                    mip.PostalProvince = resultconv.Data[i].city;
                                    mip.PostalPostalCode = resultconv.Data[i].postal_code;
                                    mip.PhysicalAddress1 = resultconv.Data[i].postal_address_line_1;
                                    mip.PhysicalAddress2 = resultconv.Data[i].postal_address_line_2;
                                    mip.PhysicalAddress3 = resultconv.Data[i].suburb;
                                    mip.PhysicalProvince = resultconv.Data[i].city;
                                    mip.PhysicalPostalCode = resultconv.Data[i].postal_code;
                                    mip.EmailAddress = resultconv.Data[i].email;
                                    mip.HomeTelephoneNumber = resultconv.Data[i].alternative_number;
                                    mip.WorkTelephoneNumber = "";
                                    mip.CellphoneNumber = resultconv.Data[i].cell_number;
                                    mip.PastYearAdmission = _appSettings.PastYearAdmission; //config
                                    mip.AdditionalBuyUp = (string)json["data"][0]["Additional_Buy_Up"];
                                    mip.PROCESS_DTTM = _appSettings.ProcessDTTM; //config



                                    PolicyDetails pol = new PolicyDetails();
                                   

                                    pol.DependantInformation = new DependantInfo();
                                
                                    pol.PolicyNumber = resultconv.Data[i].policynumber;
                                    pol.PolicyInceptionDate = resultconv.Data[i].inception_date;
                                    pol.DebitOrderDate = resultconv.Data[i].debit_order_date;
                                    pol.BrokerCode = (string)json["data"][0]["Broker_Number"];  //MIP module

                                    pol.ProductCode = resultconv.Data[i].productcode; ///check
                                    pol.AccountHolder = resultconv.Data[i].account_holder_name;
                                    pol.AccountTypeCode = resultconv.Data[i].account_type;
                                    if (resultconv.Data[i].account_type == "Cheque")
                                    {
                                        pol.AccountTypeCode = "1";
                                    }
                                    else if(resultconv.Data[i].account_type == "Savings")
                                    {
                                        pol.AccountTypeCode = "2";
                                    }
                                    pol.AccountNumber = resultconv.Data[i].account_number;
                                    pol.BranchCode = (string)json["data"][0]["branch_code_clm"]; //MIP side
                                    pol.ClaimAccountHolder = resultconv.Data[i].account_holder_name;
                                    if (resultconv.Data[i].account_type == "Cheque")
                                    {
                                        pol.ClaimAccountTypeCode = "1";
                                    }
                                    else if (resultconv.Data[i].account_type == "Savings")
                                    {
                                        pol.ClaimAccountTypeCode = "2";
                                    }
                                    pol.ClaimAccountNumber = resultconv.Data[i].account_number;
                                    pol.ClaimBranchCode = (string)json["data"][0]["branch_code_clm"]; //mip
                                    pol.AccHolderID = "";
                                    pol.AVSComp = _appSettings.AVSCode;//config set
                                    pol.DOMandate = _appSettings.DOMandate;//config
                                    pol.MarketingAuth = resultconv.Data[i].created_time;
                                    pol.EthnicGroup = resultconv.Data[i].race;
                                    pol.Gender = resultconv.Data[i].gender[0].ToString();//m/f
                                    pol.PREM = resultconv.Data[i].Premium_Test;





                                    DependantInfo depInfo = new DependantInfo();
                                    var depd = new DependantDet();


                                    if (resultconv.Data[i].firstname != null)
                                    {
                                       depd.DEP = "0";
                                       depd.DependantInceptionDate = resultconv.Data[i].inception_date;
                                       depd.DependantFirstName = resultconv.Data[i].firstname;
                                       depd.DependantSurname = resultconv.Data[i].surname;
                                        depd.DependantGender = resultconv.Data[i].gender[0].ToString();//M/F
                                        depd.IDNumber = resultconv.Data[i].id_number;
                                        depd.PassportNumber = resultconv.Data[i].id_number;
                                        depd.DateOfBirth = resultconv.Data[i].date_of_birth;
                                        depd.DependantRelationship = _appSettings.Main;
                                        depd.PREM = resultconv.Data[i].Main_Member_cost;
                                        depd.DependantTypeDescription = resultconv.Data[i].gender;
                                     
                                        if (depd.DependantRelationship == "Main_Member")
                                        {
                                            depd.DependantTypeCode = _appSettings.Main;
                                        }
                                        else if (depd.DependantRelationship == "Adult")
                                        {
                                            depd.DependantTypeCode = _appSettings.AdultType;
                                        }
                                        else if (depd.DependantRelationship == "Spouse")
                                        {
                                            depd.DependantTypeCode = _appSettings.SpouseType;
                                        }
                                        else if(depd.DependantRelationship == "Child")
                                        {
                                            depd.DependantTypeCode = _appSettings.ChildType;
                                        }

                                       depd.DependantResignationDate = "";
                                       depd.PastYearAdmission = "";
                                      
                                       depd.EthnicGroup = resultconv.Data[i].race;
                                       depd.AdditionalBuyUp = (string)json["data"][0]["dep_additional_buy_up_1"];//mip
                                       depd.OID = _appSettings.OID;//Set in config


                                       depInfo.DependantDetails = new List<DependantDet> { depd };

                                        pol.DependantInformation = depInfo;
                                    }

                                    if (resultconv.Data[i].dep1name != null)
                                    {
                                        depd.DEP = "1";
                                        depd.DependantInceptionDate = resultconv.Data[i].inception_date;
                                        depd.DependantFirstName = resultconv.Data[i].dep1name;
                                        depd.DependantSurname = resultconv.Data[i].dep_1_surname;
                                        depd.DependantGender = resultconv.Data[i].dep_1_gender[0].ToString();//M/F
                                        depd.IDNumber = resultconv.Data[i].dep_1_date_of_birth;
                                        depd.PassportNumber = resultconv.Data[i].dep_1_date_of_birth;
                                        depd.DateOfBirth = resultconv.Data[i].dep_1_date_of_birth;
                                        depd.DependantRelationship = resultconv.Data[i].Dep_1_Type;
                                        if (depd.DependantRelationship == "Adult" || depd.DependantRelationship == "Spouse")
                                        {
                                            depd.PREM = resultconv.Data[i].Adult_Dependent_Cost;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.PREM = resultconv.Data[i].Child_dependent_cost;
                                        }
                                        depd.DependantTypeDescription = resultconv.Data[i].dep_1_gender;
                                        if (depd.DependantRelationship == "Main_Member")
                                        {
                                            depd.DependantTypeCode = _appSettings.Main;
                                        }
                                        else if (depd.DependantRelationship == "Adult")
                                        {
                                            depd.DependantTypeCode = _appSettings.AdultType;
                                        }
                                        else if (depd.DependantRelationship == "Spouse")
                                        {
                                            depd.DependantTypeCode = _appSettings.SpouseType;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.DependantTypeCode = _appSettings.ChildType;
                                        }
                                        depd.DependantResignationDate = "";
                                        depd.PastYearAdmission = "";
                                       
                                        depd.EthnicGroup = resultconv.Data[i].race;
                                        depd.AdditionalBuyUp = (string)json["data"][0]["dep_additional_buy_up_1"];//mip
                                        depd.OID = _appSettings.OID;//Set in config


                                        depInfo.DependantDetails = new List<DependantDet> { depd };

                                        pol.DependantInformation = depInfo;
                                       
                                        var polc = new MemberDetail();
                                        polc.PolicyDetails = pol;
                                        polc.MemberDetails = mip;
                                    }

                                    if (resultconv.Data[i].dep2name != null)
                                    {
                                        depd.DEP = "2";
                                        depd.DependantInceptionDate = resultconv.Data[i].inception_date;
                                        depd.DependantFirstName = resultconv.Data[i].dep2name;
                                        depd.DependantSurname = resultconv.Data[i].dep_2_surname;
                                        depd.DependantGender = resultconv.Data[i].dep_2_gender[0].ToString();
                                        depd.IDNumber = resultconv.Data[i].dep_2_date_of_birth;
                                        depd.PassportNumber = resultconv.Data[i].dep_2_date_of_birth;
                                        depd.DateOfBirth = resultconv.Data[i].dep_2_date_of_birth;
                                        depd.DependantRelationship = resultconv.Data[i].Dep_2_Type;
                                        if (depd.DependantRelationship == "Adult" || depd.DependantRelationship == "Spouse")
                                        {
                                            depd.PREM = resultconv.Data[i].Adult_Dependent_Cost;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.PREM = resultconv.Data[i].Child_dependent_cost;
                                        }
                                        depd.DependantTypeDescription = resultconv.Data[i].dep_2_gender;
                                        if (depd.DependantRelationship == "Main_Member")
                                        {
                                            depd.DependantTypeCode = _appSettings.Main;
                                        }
                                        else if (depd.DependantRelationship == "Adult")
                                        {
                                            depd.DependantTypeCode = _appSettings.AdultType;
                                        }
                                        else if (depd.DependantRelationship == "Spouse")
                                        {
                                            depd.DependantTypeCode = _appSettings.SpouseType;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.DependantTypeCode = _appSettings.ChildType;
                                        }
                                        depd.DependantResignationDate = "";
                                        depd.PastYearAdmission = "";
                                       
                                        depd.EthnicGroup = resultconv.Data[i].race;
                                        depd.AdditionalBuyUp = (string)json["data"][0]["dep_additional_buy_up_2"];
                                        depd.OID = _appSettings.OID;

                                        depInfo.DependantDetails = new List<DependantDet> { depd };

                                        pol.DependantInformation = depInfo;
                                    }


                                    if (resultconv.Data[i].dep3name != null)
                                    {
                                        depd.DEP = "3";
                                        depd.DependantInceptionDate = resultconv.Data[i].inception_date;
                                        depd.DependantFirstName = resultconv.Data[i].dep3name;
                                        depd.DependantSurname = resultconv.Data[i].dep_3_surname;
                                        depd.DependantGender = resultconv.Data[i].dep_3_gender[0].ToString();
                                        depd.IDNumber = resultconv.Data[i].dep_3_date_of_birth;
                                        depd.PassportNumber = resultconv.Data[i].dep_3_date_of_birth;
                                        depd.DateOfBirth = resultconv.Data[i].dep_3_date_of_birth;
                                        depd.DependantRelationship = resultconv.Data[i].Dep_3_Type;
                                        if (depd.DependantRelationship == "Adult" || depd.DependantRelationship == "Spouse")
                                        {
                                            depd.PREM = resultconv.Data[i].Adult_Dependent_Cost;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.PREM = resultconv.Data[i].Child_dependent_cost;
                                        }
                                        depd.DependantTypeDescription = resultconv.Data[i].dep_3_gender;
                                        if (depd.DependantRelationship == "Main_Member")
                                        {
                                            depd.DependantTypeCode = _appSettings.Main;
                                        }
                                        else if (depd.DependantRelationship == "Adult")
                                        {
                                            depd.DependantTypeCode = _appSettings.AdultType;
                                        }
                                        else if (depd.DependantRelationship == "Spouse")
                                        {
                                            depd.DependantTypeCode = _appSettings.SpouseType;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.DependantTypeCode = _appSettings.ChildType;
                                        }
                                        depd.DependantResignationDate = "";
                                        depd.PastYearAdmission = "";
                                       
                                        depd.EthnicGroup = resultconv.Data[i].race;
                                        depd.AdditionalBuyUp = (string)json["data"][0]["dep_additional_buy_up_3"];
                                        depd.OID = _appSettings.OID;

                                        depInfo.DependantDetails = new List<DependantDet> { depd };

                                        pol.DependantInformation = depInfo;
                                    }

                                    if (resultconv.Data[i].dep4name != null)
                                    {
                                        depd.DEP = "4";
                                        depd.DependantInceptionDate = resultconv.Data[i].inception_date;
                                        depd.DependantFirstName = resultconv.Data[i].dep4name;
                                        depd.DependantSurname = resultconv.Data[i].dep_4_surname;
                                        depd.DependantGender = resultconv.Data[i].dep_4_gender[0].ToString();
                                        depd.IDNumber = resultconv.Data[i].dep_4_date_of_birth;
                                        depd.PassportNumber = resultconv.Data[i].dep_4_date_of_birth;
                                        depd.DateOfBirth = resultconv.Data[i].dep_4_date_of_birth;
                                        depd.DependantRelationship = resultconv.Data[i].Dep_4_Type;
                                        if (depd.DependantRelationship == "Adult" || depd.DependantRelationship == "Spouse")
                                        {
                                            depd.PREM = resultconv.Data[i].Adult_Dependent_Cost;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.PREM = resultconv.Data[i].Child_dependent_cost;
                                        }
                                        depd.DependantTypeDescription = resultconv.Data[i].dep_4_gender;
                                        if (depd.DependantRelationship == "Main_Member")
                                        {
                                            depd.DependantTypeCode = _appSettings.Main;
                                        }
                                        else if (depd.DependantRelationship == "Adult")
                                        {
                                            depd.DependantTypeCode = _appSettings.AdultType;
                                        }
                                        else if (depd.DependantRelationship == "Spouse")
                                        {
                                            depd.DependantTypeCode = _appSettings.SpouseType;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.DependantTypeCode = _appSettings.ChildType;
                                        }
                                        depd.DependantResignationDate = "";
                                        depd.PastYearAdmission = "";
                                       
                                        depd.EthnicGroup = resultconv.Data[i].race;
                                        depd.AdditionalBuyUp = (string)json["data"][0]["dep_additional_buy_up_4"];
                                        depd.OID = _appSettings.OID;

                                        depInfo.DependantDetails = new List<DependantDet> { depd };

                                        pol.DependantInformation = depInfo;
                                       // mip.PolicyDetails = pol;
                                    }

                                    if (resultconv.Data[i].dep5name != null)
                                    {
                                        depd.DEP = "5";
                                        depd.DependantInceptionDate = resultconv.Data[i].inception_date;
                                        depd.DependantFirstName = resultconv.Data[i].dep5name;
                                        depd.DependantSurname = resultconv.Data[i].dep_5_surname;
                                        depd.DependantGender = resultconv.Data[i].dep_5_gender[0].ToString();
                                        depd.IDNumber = resultconv.Data[i].dep_5_date_of_birth;
                                        depd.PassportNumber = resultconv.Data[i].dep_5_date_of_birth;
                                        depd.DateOfBirth = resultconv.Data[i].dep_5_date_of_birth;
                                        depd.DependantRelationship = resultconv.Data[i].Dep_5_Type;
                                        if (depd.DependantRelationship == "Adult" || depd.DependantRelationship == "Spouse")
                                        {
                                            depd.PREM = resultconv.Data[i].Adult_Dependent_Cost;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.PREM = resultconv.Data[i].Child_dependent_cost;
                                        }
                                        depd.DependantTypeDescription = resultconv.Data[i].dep_5_gender;
                                        if (depd.DependantRelationship == "Main_Member")
                                        {
                                            depd.DependantTypeCode = _appSettings.Main;
                                        }
                                        else if (depd.DependantRelationship == "Adult")
                                        {
                                            depd.DependantTypeCode = _appSettings.AdultType;
                                        }
                                        else if (depd.DependantRelationship == "Spouse")
                                        {
                                            depd.DependantTypeCode = _appSettings.SpouseType;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.DependantTypeCode = _appSettings.ChildType;
                                        }
                                        depd.DependantResignationDate = "";
                                        depd.PastYearAdmission = "";
                                       // depd.PREM = "";//set
                                        depd.EthnicGroup = resultconv.Data[i].race;
                                        depd.AdditionalBuyUp = (string)json["data"][0]["dep_additional_buy_up_5"];
                                        depd.OID = _appSettings.OID;

                                        depInfo.DependantDetails = new List<DependantDet> { depd };

                                        pol.DependantInformation = depInfo;
                                    }

                                    if (resultconv.Data[i].dep6name != null)
                                    {
                                        depd.DEP = "6";
                                        depd.DependantInceptionDate = resultconv.Data[i].inception_date;
                                        depd.DependantFirstName = resultconv.Data[i].dep6name;
                                        depd.DependantSurname = resultconv.Data[i].dep_6_surname;
                                        depd.DependantGender = resultconv.Data[i].dep_6_gender[0].ToString();
                                        depd.IDNumber = resultconv.Data[i].dep_6_date_of_birth;
                                        depd.PassportNumber = resultconv.Data[i].dep_6_date_of_birth;
                                        depd.DateOfBirth = resultconv.Data[i].dep_6_date_of_birth;
                                        depd.DependantRelationship = resultconv.Data[i].Dep_6_Type;
                                        if (depd.DependantRelationship == "Adult" || depd.DependantRelationship == "Spouse")
                                        {
                                            depd.PREM = resultconv.Data[i].Adult_Dependent_Cost;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.PREM = resultconv.Data[i].Child_dependent_cost;
                                        }
                                        depd.DependantTypeDescription = resultconv.Data[i].dep_6_gender;
                                        if (depd.DependantRelationship == "Main_Member")
                                        {
                                            depd.DependantTypeCode = _appSettings.Main;
                                        }
                                        else if (depd.DependantRelationship == "Adult")
                                        {
                                            depd.DependantTypeCode = _appSettings.AdultType;
                                        }
                                        else if (depd.DependantRelationship == "Spouse")
                                        {
                                            depd.DependantTypeCode = _appSettings.SpouseType;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.DependantTypeCode = _appSettings.ChildType;
                                        }
                                        depd.DependantResignationDate = "";
                                        depd.PastYearAdmission = "";
                                      
                                        depd.EthnicGroup = resultconv.Data[i].race;
                                        depd.AdditionalBuyUp = (string)json["data"][0]["dep_additional_buy_up_6"];
                                        depd.OID = _appSettings.OID;

                                        depInfo.DependantDetails = new List<DependantDet> { depd };

                                        pol.DependantInformation = depInfo;
                                    }

                                    if (resultconv.Data[i].dep7name != null)
                                    {

                                        
                                        
                                        depd.DEP = "7";
                                        depd.DependantInceptionDate = resultconv.Data[i].inception_date;
                                        depd.DependantFirstName = resultconv.Data[i].dep7name;
                                        depd.DependantSurname = resultconv.Data[i].dep_7_surname;
                                        depd.DependantGender = resultconv.Data[i].dep_7_gender[0].ToString();
                                        depd.IDNumber = resultconv.Data[i].dep_7_date_of_birth;
                                        depd.PassportNumber = resultconv.Data[i].dep_7_date_of_birth;
                                        depd.DateOfBirth = resultconv.Data[i].dep_7_date_of_birth;
                                        depd.DependantRelationship = resultconv.Data[i].Dep_7_Type;
                                        if (depd.DependantRelationship == "Adult" || depd.DependantRelationship == "Spouse")
                                        {
                                            depd.PREM = resultconv.Data[i].Adult_Dependent_Cost;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.PREM = resultconv.Data[i].Child_dependent_cost;
                                        }
                                        depd.DependantTypeDescription = resultconv.Data[i].dep_7_gender;
                                        if (depd.DependantRelationship == "Main_Member")
                                        {
                                            depd.DependantTypeCode = _appSettings.Main;
                                        }
                                        else if (depd.DependantRelationship == "Adult")
                                        {
                                            depd.DependantTypeCode = _appSettings.AdultType;
                                        }
                                        else if (depd.DependantRelationship == "Spouse")
                                        {
                                            depd.DependantTypeCode = _appSettings.SpouseType;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.DependantTypeCode = _appSettings.ChildType;
                                        }
                                        depd.DependantResignationDate = "";
                                        depd.PastYearAdmission = "";
                                       
                                        depd.EthnicGroup = resultconv.Data[i].race;
                                        depd.AdditionalBuyUp = (string)json["data"][0]["dep_additional_buy_up_7"];
                                        depd.OID = _appSettings.OID;

                                        depInfo.DependantDetails = new List<DependantDet> { depd };

                                        pol.DependantInformation = depInfo;
                                    }

                                    if (resultconv.Data[i].dep8name != null)
                                    {

                                        
                                        
                                        depd.DEP = "8";
                                        depd.DependantInceptionDate = resultconv.Data[i].inception_date;
                                        depd.DependantFirstName = resultconv.Data[i].dep8name;
                                        depd.DependantSurname = resultconv.Data[i].dep_8_surname;
                                        depd.DependantGender = resultconv.Data[i].dep_8_gender[0].ToString();
                                        depd.IDNumber = resultconv.Data[i].dep_8_date_of_birth;
                                        depd.PassportNumber = resultconv.Data[i].dep_8_date_of_birth;
                                        depd.DateOfBirth = resultconv.Data[i].dep_8_date_of_birth;
                                        depd.DependantRelationship = resultconv.Data[i].Dep_8_Type;
                                        if (depd.DependantRelationship == "Adult" || depd.DependantRelationship == "Spouse")
                                        {
                                            depd.PREM = resultconv.Data[i].Adult_Dependent_Cost;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.PREM = resultconv.Data[i].Child_dependent_cost;
                                        }
                                        depd.DependantTypeDescription = resultconv.Data[i].dep_8_gender;
                                        if (depd.DependantRelationship == "Main_Member")
                                        {
                                            depd.DependantTypeCode = _appSettings.Main;
                                        }
                                        else if (depd.DependantRelationship == "Adult")
                                        {
                                            depd.DependantTypeCode = _appSettings.AdultType;
                                        }
                                        else if (depd.DependantRelationship == "Spouse")
                                        {
                                            depd.DependantTypeCode = _appSettings.SpouseType;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.DependantTypeCode = _appSettings.ChildType;
                                        }
                                        depd.DependantResignationDate = "";
                                        depd.PastYearAdmission = "";
                                       
                                        depd.EthnicGroup = resultconv.Data[i].race;
                                        depd.AdditionalBuyUp = (string)json["data"][0]["dep_additional_buy_up_8"];
                                        depd.OID = _appSettings.OID;

                                        depInfo.DependantDetails = new List<DependantDet> { depd };

                                        pol.DependantInformation = depInfo;
                                    }

                                    if (resultconv.Data[i].dep9name != null)
                                    {

                                        
                                        
                                        depd.DEP = "9";
                                        depd.DependantInceptionDate = resultconv.Data[i].inception_date;
                                        depd.DependantFirstName = resultconv.Data[i].dep9name;
                                        depd.DependantSurname = resultconv.Data[i].dep_9_surname;
                                        depd.DependantGender = resultconv.Data[i].dep_9_gender[0].ToString();
                                        depd.IDNumber = resultconv.Data[i].dep_9_date_of_birth;
                                        depd.PassportNumber = resultconv.Data[i].dep_9_date_of_birth;
                                        depd.DateOfBirth = resultconv.Data[i].dep_9_date_of_birth;
                                        depd.DependantRelationship = resultconv.Data[i].Dep_9_Type;
                                        if (depd.DependantRelationship == "Adult" || depd.DependantRelationship == "Spouse")
                                        {
                                            depd.PREM = resultconv.Data[i].Adult_Dependent_Cost;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.PREM = resultconv.Data[i].Child_dependent_cost;
                                        }
                                        depd.DependantTypeDescription = resultconv.Data[i].dep_9_gender;
                                        if (depd.DependantRelationship == "Main_Member")
                                        {
                                            depd.DependantTypeCode = _appSettings.Main;
                                        }
                                        else if (depd.DependantRelationship == "Adult")
                                        {
                                            depd.DependantTypeCode = _appSettings.AdultType;
                                        }
                                        else if (depd.DependantRelationship == "Spouse")
                                        {
                                            depd.DependantTypeCode = _appSettings.SpouseType;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.DependantTypeCode = _appSettings.ChildType;
                                        }
                                        depd.DependantResignationDate = "";
                                        depd.PastYearAdmission = "";
                                       
                                        depd.EthnicGroup = resultconv.Data[i].race;
                                        depd.AdditionalBuyUp = (string)json["data"][0]["dep_additional_buy_up_9"];
                                        depd.OID = _appSettings.OID;

                                        depInfo.DependantDetails = new List<DependantDet> { depd };

                                        pol.DependantInformation = depInfo;
                                    }

                                    if (resultconv.Data[i].dep10name != null)
                                    {

                                        depd.DEP = "10";
                                        depd.DependantInceptionDate = resultconv.Data[i].inception_date;
                                        depd.DependantFirstName = resultconv.Data[i].dep10name;
                                        depd.DependantSurname = resultconv.Data[i].dep_10_surname;
                                        depd.DependantGender = resultconv.Data[i].dep_10_gender[0].ToString();
                                        depd.IDNumber = resultconv.Data[i].dep_10_date_of_birth;
                                        depd.PassportNumber = resultconv.Data[i].dep_10_date_of_birth;
                                        depd.DateOfBirth = resultconv.Data[i].dep_10_date_of_birth;
                                        depd.DependantRelationship = resultconv.Data[i].Dep_10_Type;
                                        if (depd.DependantRelationship == "Adult" || depd.DependantRelationship == "Spouse")
                                        {
                                            depd.PREM = resultconv.Data[i].Adult_Dependent_Cost;
                                        }
                                        else if (depd.DependantRelationship == "Child")
                                        {
                                            depd.PREM = resultconv.Data[i].Child_dependent_cost;
                                        }
                                        depd.DependantTypeDescription = resultconv.Data[i].dep_10_gender;
                                         if (depd.DependantRelationship == "Main_Member")
                                        {
                                            depd.DependantTypeCode = _appSettings.Main;
                                        }
                                        else if (depd.DependantRelationship == "Adult")
                                        {
                                            depd.DependantTypeCode = _appSettings.AdultType;
                                        }
                                        else if (depd.DependantRelationship == "Spouse")
                                        {
                                            depd.DependantTypeCode = _appSettings.SpouseType;
                                        }
                                        else if(depd.DependantRelationship == "Child")
                                        {
                                            depd.DependantTypeCode = _appSettings.ChildType;
                                        }
                                        depd.DependantResignationDate = "";
                                        depd.PastYearAdmission = "";
                                       
                                        depd.EthnicGroup = resultconv.Data[i].race;
                                        depd.AdditionalBuyUp = (string)json["data"][0]["dep_additional_buy_up_10"];
                                        depd.OID = _appSettings.OID;

                                        depInfo.DependantDetails = new List<DependantDet> { depd };

                                        pol.DependantInformation = depInfo;
                                    }

                                   
                                    //table = "Mi";
                                    //dbInsert = _dapper.QuerySingle<int>(con, Queries.InsertMi, CommandType.StoredProcedure, resultconv.Data[i]);

                                    MemberDetail mips = new MemberDetail();

                                    mips.PolicyDetails = pol;
                                    var idk = new MemberDetail();
                                    idk.MemberDetails = mip;
                                    idk.PolicyDetails = pol;
                                    

                                    string data = JsonConvert.SerializeObject(idk);

                                    var options = new RestClientOptions(_appSettings.mip)
                                    {
                                        MaxTimeout = -1,
                                    };
                                    var client = new RestClient(options);
                                    var request = new RestRequest("/eip_tst/rest/client_route/hrf/kaelo/zohomembership/request", Method.Post);
                                    //request.Method = Method.Post;
                                    request.AddHeader("Content-Type", "text/html");
                                    string user = _appSettings.username; 
                                    string pass = _appSettings.password;

                                 

                                    string credentials = $"{user}:{pass}";
                                    byte[] credentialsBytes = Encoding.UTF8.GetBytes(credentials);
                                    string credentialsBase64 = Convert.ToBase64String(credentialsBytes);
                                    request.AddHeader("Authorization", "Basic " + credentialsBase64);
                                    request.AddBody(data);
                                    var response = await client.ExecuteAsync(request);

                                    var mipResponse = JsonConvert.DeserializeObject<MipResponse>(response.Content);

                                    int startIndex = mipResponse.data[1].ERROR_MESSAGE.IndexOf("TOTAL PREMIUMS:") + "TOTAL PREMIUMS:".Length;
                                    string premiumsValue = mipResponse.data[1].ERROR_MESSAGE.Substring(startIndex);

                                    var zohoclient1= new RestClient();


                                    var putData = new ZohoPutRequest();
                                    var zohoPutData = new ZohoPut();
                                    putData.data = new List<ZohoPut>();

                                    zohoPutData.mip_medstat_number = mipResponse.data[0].POLICY_NBR;
                                    zohoPutData.mipresponse = mipResponse.data[0].ERROR.ToString();
                                    zohoPutData.mipresponsemessage = mipResponse.data[0].ERROR_MESSAGE;
                                    zohoPutData.mip_policy_premium = premiumsValue;

                                    zohoPutData.mip_main_member_premium = premiumsValue;
                                    putData.data.Add(zohoPutData);

                                    var zoho = new RestRequest(zoho_base_url + resultconv.Data[0].id);
                                    zoho.Method = Method.Put;
                                    zoho.AddHeader("Authorization", "Zoho-oauthtoken " + accesstoken.access_token);



                                   

                                    zoho.AddJsonBody<ZohoPutRequest>(putData);

                                  

                                    var ress = await zohoclient1.ExecuteAsync(zoho);

                                    Log.Information("Post To Zoho: "+ress.Content);

                                    string query = "Execute dbo.InsertMi  @id, @policynumber, @Comp_Code, @Product1, @Product, @title, @surname, @firstname , @id_number , @id_type   , @Date_Of_Birth , @gender , @Alternative_Number,  @Cell_number   , @email , @Postal_Address_Line_1 , @Postal_Address_Line_2 , @Suburb , @City  , @Postal_Code   , @Inception_Date , @Broker_Number , @Mandate_Reference , @Account_Type  , @Account_Number , @BranchCode , @Account_Holder_Name   , @Debit_Order_Date , @account_type_clm  , @account_num_clm   , @branch_code_clm   , @account_name_clm  , @Additional_Buy_Up , @Premiums  , @dep1name  , @Dep_1_Surname , @Dep_1_unique  , @Dep_1_Date_of_Birth   , @Dep_1_Gender  , @Dep_1_Type , @dep_relationship_1 , @dep_additional_buy_up_1   , @dep2name  , @Dep_2_Surname , @Dep_2_unique  , @Dep_2_Date_of_Birth   , @Dep_2_Gender  , @Dep_2_Type , @dep_relationship_2 , @dep_additional_buy_up_2   , @dep3name  , @Dep_3_Surname , @Dep_3_unique  , @Dep_3_Date_of_Birth   , @Dep_3_Type , @Dep_3_Gender  , @dep_relationship_3 , @dep_additional_buy_up_3   , @dep4name  , @Dep_4_Surname , @Dep_4_unique  , @Dep_4_Gender  , @Dep_4_Date_of_Birth   , @Dep_4_Type , @dep_relationship_4 , @dep_additional_buy_up_4   , @dep5name  , @Dep_5_unique  , @Dep_5_Surname , @Dep_5_Date_of_Birth   , @Dep_5_Gender  , @Dep_5_Type , @dep_relationship_5 , @dep_additional_buy_up_5   , @dep6name  , @Dep_6_Surname , @Dep_6_unique  , @Dep_6_Date_of_Birth   , @Dep_6_Gender  , @Dep_6_Type , @dep_relationship_6 , @dep7name  , @dep_additional_buy_up_6   , @Dep_7_Surname , @Dep_7_unique  , @Dep_7_Date_of_Birth   , @Dep_7_Gender  , @Dep_7_Type , @dep_relationship_7 , @dep_additional_buy_up_7   , @dep8name  , @Dep_8_Surname , @Dep_8_unique  , @Dep_8_Date_of_Birth   , @Dep_8_Gender  , @Dep_8_Type , @dep_relationship_8 , @dep_additional_buy_up_8   , @dep9name  , @Dep_9_Surname , @Dep_9_unique  , @Dep_9_Date_of_Birth   , @Dep_9_Gender  , @Dep_9_Type , @dep_relationship_9 , @dep_additional_buy_up_9   , @dep10name , @Dep_10_Surname , @Dep_10_unique , @Dep_10_Date_of_Birth  , @Dep_10_Gender , @Dep_10_Type   , @dep_relationship_10   ,@dep_additional_buy_up_10 ,@compCode  ";
                                      
                                    var dbRes = con.QuerySingle<int>(query, new 
                                    {
                                        @id = "",
                                        @policynumber   = resultconv.Data[0].policynumber,
                                        @Comp_Code  = resultconv.Data[0].Comp_Code,
                                        @Product1   = resultconv.Data[0].product,
                                        @Product    = resultconv.Data[0].productcode,//check
                                        @title  = resultconv.Data[0].title,
                                        @surname    = resultconv.Data[0].surname,
                                        @firstname  = resultconv.Data[0].firstname,
                                        @id_number  = resultconv.Data[0].id_number,
                                        @id_type    = resultconv.Data[0].id_type,
                                        @Date_Of_Birth  = resultconv.Data[0].date_of_birth,
                                        @gender = resultconv.Data[0].gender,
                                        @Alternative_Number = resultconv.Data[0].alternative_number,
                                        @Cell_number    = resultconv.Data[0].cell_number,
                                        @email  = resultconv.Data[0].email,
                                        @Postal_Address_Line_1  = resultconv.Data[0].postal_address_line_1,
                                        @Postal_Address_Line_2  = resultconv.Data[0].postal_address_line_2,
                                        @Suburb = resultconv.Data[0].suburb,
                                        @City   = resultconv.Data[0].city,
                                        @Postal_Code    = resultconv.Data[0].postal_code,
                                        @Inception_Date = resultconv.Data[0].inception_date,
                                        @Broker_Number  = resultconv.Data[0].Broker_Number,
                                        @Mandate_Reference  = resultconv.Data[0].Mandate,//check
                                        @Account_Type   = resultconv.Data[0].account_type,
                                        @Account_Number = resultconv.Data[0].account_number,
                                        @BranchCode = resultconv.Data[0].branch_code_clm,
                                        @Account_Holder_Name    = resultconv.Data[0].account_holder_name,
                                        @Debit_Order_Date  = resultconv.Data[0].debit_order_date,
                                        @account_type_clm   = resultconv.Data[0].account_type,
                                        @account_num_clm    = resultconv.Data[0].account_number,
                                        @branch_code_clm    = resultconv.Data[0].branch_code_clm,
                                        @account_name_clm   = resultconv.Data[0].account_holder_name,
                                        @Additional_Buy_Up  = resultconv.Data[0].Additional_Buy_Up,
                                        @Premiums   = resultconv.Data[0].Premium_Test,
                                        @dep1name = resultconv.Data[i].dep1name,
                                        @Dep_1_Surname = resultconv.Data[i].dep_1_surname,
                                        @Dep_1_unique = resultconv.Data[i].dep_1_date_of_birth,
                                        @Dep_1_Date_of_Birth = resultconv.Data[i].dep_1_date_of_birth,
                                        @Dep_1_Gender = resultconv.Data[i].dep_1_gender,
                                        @Dep_1_Type = resultconv.Data[i].Dep_1_Type,
                                        @dep_relationship_1 = resultconv.Data[i].Dep_1_Type,
                                        @dep_additional_buy_up_1 = resultconv.Data[i].dep_additional_buy_up_1,
                                        @dep2name = resultconv.Data[i].dep2name,
                                        @Dep_2_Surname = resultconv.Data[i].dep_2_surname,
                                        @Dep_2_unique = resultconv.Data[i].dep_2_date_of_birth,
                                        @Dep_2_Date_of_Birth = resultconv.Data[i].dep_2_date_of_birth,
                                        @Dep_2_Gender = resultconv.Data[i].dep_2_gender,
                                        @Dep_2_Type = resultconv.Data[i].Dep_2_Type,
                                        @dep_relationship_2 = resultconv.Data[i].Dep_2_Type,
                                        @dep_additional_buy_up_2 = resultconv.Data[i].dep_additional_buy_up_2,
                                        @dep3name = resultconv.Data[i].dep3name,
                                        @Dep_3_Surname = resultconv.Data[i].dep_3_surname,
                                        @Dep_3_unique = resultconv.Data[i].dep_3_date_of_birth,
                                        @Dep_3_Date_of_Birth = resultconv.Data[i].dep_3_date_of_birth,
                                        @Dep_3_Gender = resultconv.Data[i].dep_3_gender,
                                        @Dep_3_Type = resultconv.Data[i].Dep_3_Type,
                                        @dep_relationship_3 = resultconv.Data[i].Dep_3_Type,
                                        @dep_additional_buy_up_3 = resultconv.Data[i].dep_additional_buy_up_3,
                                        @dep4name = resultconv.Data[i].dep4name,
                                        @Dep_4_Surname = resultconv.Data[i].dep_4_surname,
                                        @Dep_4_unique = resultconv.Data[i].dep_4_date_of_birth,
                                        @Dep_4_Date_of_Birth = resultconv.Data[i].dep_4_date_of_birth,
                                        @Dep_4_Gender = resultconv.Data[i].dep_4_gender,
                                        @Dep_4_Type = resultconv.Data[i].Dep_4_Type,
                                        @dep_relationship_4 = resultconv.Data[i].Dep_4_Type,
                                        @dep_additional_buy_up_4 = resultconv.Data[i].dep_additional_buy_up_4,
                                        @dep5name = resultconv.Data[i].dep5name,
                                        @Dep_5_Surname = resultconv.Data[i].dep_5_surname,
                                        @Dep_5_unique = resultconv.Data[i].dep_5_date_of_birth,
                                        @Dep_5_Date_of_Birth = resultconv.Data[i].dep_5_date_of_birth,
                                        @Dep_5_Gender = resultconv.Data[i].dep_5_gender,
                                        @Dep_5_Type = resultconv.Data[i].Dep_5_Type,
                                        @dep_relationship_5 = resultconv.Data[i].Dep_5_Type,
                                        @dep_additional_buy_up_5 = resultconv.Data[i].dep_additional_buy_up_5,
                                        @dep6name = resultconv.Data[i].dep6name,
                                        @Dep_6_Surname = resultconv.Data[i].dep_6_surname,
                                        @Dep_6_unique = resultconv.Data[i].dep_6_date_of_birth,
                                        @Dep_6_Date_of_Birth = resultconv.Data[i].dep_6_date_of_birth,
                                        @Dep_6_Gender = resultconv.Data[i].dep_6_gender,
                                        @Dep_6_Type = resultconv.Data[i].Dep_6_Type,
                                        @dep_relationship_6 = resultconv.Data[i].Dep_6_Type,
                                        @dep_additional_buy_up_6 = resultconv.Data[i].dep_additional_buy_up_6,
                                        @dep7name = resultconv.Data[i].dep7name,
                                        @Dep_7_Surname = resultconv.Data[i].dep_7_surname,
                                        @Dep_7_unique = resultconv.Data[i].dep_7_date_of_birth,
                                        @Dep_7_Date_of_Birth = resultconv.Data[i].dep_7_date_of_birth,
                                        @Dep_7_Gender = resultconv.Data[i].dep_7_gender,
                                        @Dep_7_Type = resultconv.Data[i].Dep_7_Type,
                                        @dep_relationship_7 = resultconv.Data[i].Dep_7_Type,
                                        @dep_additional_buy_up_7 = resultconv.Data[i].dep_additional_buy_up_7,
                                        @dep8name = resultconv.Data[i].dep8name,
                                        @Dep_8_Surname = resultconv.Data[i].dep_8_surname,
                                        @Dep_8_unique = resultconv.Data[i].dep_8_date_of_birth,
                                        @Dep_8_Date_of_Birth = resultconv.Data[i].dep_8_date_of_birth,
                                        @Dep_8_Gender = resultconv.Data[i].dep_8_gender,
                                        @Dep_8_Type = resultconv.Data[i].Dep_8_Type,
                                        @dep_relationship_8 = resultconv.Data[i].Dep_8_Type,
                                        @dep_additional_buy_up_8 = resultconv.Data[i].dep_additional_buy_up_8,
                                        @dep9name = resultconv.Data[i].dep9name,
                                        @Dep_9_Surname = resultconv.Data[i].dep_9_surname,
                                        @Dep_9_unique = resultconv.Data[i].dep_9_date_of_birth,
                                        @Dep_9_Date_of_Birth = resultconv.Data[i].dep_9_date_of_birth,
                                        @Dep_9_Gender = resultconv.Data[i].dep_9_gender,
                                        @Dep_9_Type = resultconv.Data[i].Dep_9_Type,
                                        @dep_relationship_9 = resultconv.Data[i].Dep_9_Type,
                                        @dep_additional_buy_up_9 = resultconv.Data[i].dep_additional_buy_up_9,
                                        @dep10name = resultconv.Data[i].dep10name,
                                        @Dep_10_Surname = resultconv.Data[i].dep_10_surname,
                                        @Dep_10_unique = resultconv.Data[i].dep_10_date_of_birth,
                                        @Dep_10_Date_of_Birth = resultconv.Data[i].dep_10_date_of_birth,
                                        @Dep_10_Gender = resultconv.Data[i].dep_10_gender,
                                        @Dep_10_Type = resultconv.Data[i].Dep_10_Type,
                                        @dep_relationship_10 = resultconv.Data[i].Dep_10_Type,
                                        @dep_additional_buy_up_10 = resultconv.Data[i].dep_additional_buy_up_10,
                                        @compCode = resultconv.Data[0].Comp_Code
                                    });

                                    string query1 = "Execute dbo.InsertMip @Premium, @PolicyNumber, @Response, @Status, @IdNumber, @MedStat";

                                    var dbRes1 = con.QuerySingle<int>(query1, new
                                    {
                                        @Premium = Decimal.Parse(resultconv.Data[0].Premium_Test),
                                        @PolicyNumber = resultconv.Data[0].policynumber,
                                        @Response = response.Content,
                                        @Status = resultconv.Data[0].mip_status,
                                        @IdNumber = resultconv.Data[0].id_number,
                                        @MedStat = mipResponse.data[0].POLICY_NBR
                                    });



                                    if (!response.IsSuccessful)
                                    {
                                        Log.Information(response.Content + " " + response.ErrorMessage + " " + response.ResponseStatus + " " + response.StatusCode);
                                    }

                                   
                                }
                            }
                        }

                        await Task.Delay(timer, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    Log.Information(ex.Message);
                }
            }
        }

        public async Task<MIPAuthResponse> GetAuthToken()
        {
            try
            {
                string client_id = _appSettings.client_id;
                string client_secret = _appSettings.client_secret;
                string grant_type = "authorization_code";
                string zoho_auth_client = _appSettings.zoho_auth_client;
                string code = _appSettings.code;

                var client = new RestClient(zoho_auth_client);
                var request = new RestRequest();
                request.Method = Method.Post;
                request.AlwaysMultipartFormData = true;
                request.AddParameter("grant_type", grant_type);
                request.AddParameter("client_id", client_id);
                request.AddParameter("client_secret", client_secret);
                request.AddParameter("code", code);
                var zoho_request = await client.ExecuteAsync(request);

                MIPAuthResponse response = JsonConvert.DeserializeObject<MIPAuthResponse>(zoho_request.Content);

                File.WriteAllText(@"C:\DischemmipGap\RefTok.txt", response.refresh_token);

                return response;
            }
            catch (Exception ex)
            {
                Log.Information(ex.Message);
                return null;
            }

        }



        public async Task<MIPAuthResponse> GetRefreshToken(string refresh_token)
        {
            try
            {
                string client_id = _appSettings.client_id;
                string client_secret = _appSettings.client_secret;
                string grant_type = "refresh_token";
                string zoho_auth_client = _appSettings.zoho_auth_client;

                var client = new RestClient(zoho_auth_client);
                var request = new RestRequest();
                request.Method = Method.Post;
                request.AlwaysMultipartFormData = true;
                request.AddParameter("refresh_token", refresh_token);
                request.AddParameter("client_id", client_id);
                request.AddParameter("client_secret", client_secret);
                request.AddParameter("grant_type", grant_type);
                var zoho_request = await client.ExecuteAsync(request);

                MIPAuthResponse response = JsonConvert.DeserializeObject<MIPAuthResponse>(zoho_request.Content);

                Log.Logger.Information("Token: " + response.access_token);
                return response;

            }
            catch (Exception ex)
            {
                Log.Information(ex.Message);
                return null;
            }

        }
    }
}
