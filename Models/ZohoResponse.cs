using System;
using System.Collections.Generic;
using System.Text;

namespace MIPService.Models
{
    public class ZohoResponse
    {
        public Data[] Data { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }

    public class DependantDetails
    {
        public string dep1 { get; set; }
        public string dep2 { get; set; }
        public string dep3 { get; set; }
        public string dep4 { get; set; }
        public string dep5 { get; set; }
        public string dep6 { get; set; }
        public string dep7 { get; set; }
        public string dep8 { get; set; }
        public string dep9 { get; set; }
        public string dep10 { get; set; }
        public string dep1name { get; set; }
        public string dep2name { get; set; }
        public string dep3name { get; set; }
        public string dep4name { get; set; }
        public string dep5name { get; set; }
        public string dep6name { get; set; }
        public string dep7name { get; set; }
        public string dep8name { get; set; }
        public string dep9name { get; set; }
        public string dep10name { get; set; }
        public string dep_1_surname { get; set; }
        public string dep_2_surname { get; set; }
        public string dep_3_surname { get; set; }
        public string dep_4_surname { get; set; }
        public string dep_5_surname { get; set; }
        public string dep_6_surname { get; set; }
        public string dep_7_surname { get; set; }
        public string dep_8_surname { get; set; }
        public string dep_9_surname { get; set; }
        public string dep_10_surname { get; set; }
        public string dep_1_gender { get; set; }
        public string dep_2_gender { get; set; }
        public string dep_3_gender { get; set; }
        public string dep_4_gender { get; set; }
        public string dep_5_gender { get; set; }
        public string dep_6_gender { get; set; }
        public string dep_7_gender { get; set; }
        public string dep_8_gender { get; set; }
        public string dep_9_gender { get; set; }
        public string dep_10_gender { get; set; }
        public string dep_1_date_of_birth { get; set; }
        public string dep_2_date_of_birth { get; set; }
        public string dep_3_date_of_birth { get; set; }
        public string dep_4_date_of_birth { get; set; }
        public string dep_5_date_of_birth { get; set; }
        public string dep_6_date_of_birth { get; set; }
        public string dep_7_date_of_birth { get; set; }
        public string dep_8_date_of_birth { get; set; }
        public string dep_9_date_of_birth { get; set; }
        public string dep_10_date_of_birth { get; set; }
        public string Dep_1_Type { get; set; }
        public string Dep_2_Type { get; set; }
        public string Dep_3_Type { get; set; }
        public string Dep_4_Type { get; set; }
        public string Dep_5_Type { get; set; }
        public string Dep_6_Type { get; set; }
        public string Dep_7_Type { get; set; }
        public string Dep_8_Type { get; set; }
        public string Dep_9_Type { get; set; }
        public string Dep_10_Type { get; set; }
       
        public string DependantTypeCode { get; set; }
        public string Child_dependent_cost { get; set; }
        public string Adult_Dependent_Cost { get; set; }
        public string Main_Member_cost { get; set; }

        public string race { get; set; }
        public string dep_additional_buy_up_1 { get; set; }
        public string dep_additional_buy_up_2 { get; set; }
        public string dep_additional_buy_up_3 { get; set; }
        public string dep_additional_buy_up_4 { get; set; }
        public string dep_additional_buy_up_5 { get; set; }
        public string dep_additional_buy_up_6 { get; set; }
        public string dep_additional_buy_up_7 { get; set; }
        public string dep_additional_buy_up_8 { get; set; }
        public string dep_additional_buy_up_9 { get; set; }
        public string dep_additional_buy_up_10 { get; set; }
        public string Additional_Buy_Up { get; set; }
        public string id { get; set; }
    }

    public class Data
    {
        public MIP MIP { get; set; }
        public string policynumber { get; set; }
        public string inception_date { get; set; }
        public string debit_order_date { get; set; }
        public string Broker_Number { get; set; }
        public string Mandate { get; set; }
        public string product { get; set; }
        public string productcode { get; set; }
        public string account_holder_name { get; set; }
        public string account_type { get; set; }
        public string account_number { get; set; }
        public string branch_code_clm { get; set; }
        //public string AccHolderID { get; set; }
        //public string AVSComp { get; set; }
        //public string DOMandate { get; set; }
        public string created_time { get; set; }
        public string race { get; set; }
        public string gender { get; set; }
        public string Premium_Test { get; set; }
        //public string PastYearAdmission { get; set; }
        //public string additional_buy_up { get; set; }
        //public string Process_DTTM { get; set; }
       // public List<DependantDetail> DependantInformation { get; set; }
        public string dep1 { get; set; }
        public string dep2 { get; set; }
        public string dep3 { get; set; }
        public string dep4 { get; set; }
        public string dep5 { get; set; }
        public string dep6 { get; set; }
        public string dep7 { get; set; }
        public string dep8 { get; set; }
        public string dep9 { get; set; }
        public string dep10 { get; set; }
        public string dep1name { get; set; }
        public string dep2name { get; set; }
        public string dep3name { get; set; }
        public string dep4name { get; set; }
        public string dep5name { get; set; }
        public string dep6name { get; set; }
        public string dep7name { get; set; }
        public string dep8name { get; set; }
        public string dep9name { get; set; }
        public string dep10name { get; set; }
        public string dep_1_surname { get; set; }
        public string dep_2_surname { get; set; }
        public string dep_3_surname { get; set; }
        public string dep_4_surname { get; set; }
        public string dep_5_surname { get; set; }
        public string dep_6_surname { get; set; }
        public string dep_7_surname { get; set; }
        public string dep_8_surname { get; set; }
        public string dep_9_surname { get; set; }
        public string dep_10_surname { get; set; }
        public string dep_1_gender { get; set; }
        public string dep_2_gender { get; set; }
        public string dep_3_gender { get; set; }
        public string dep_4_gender { get; set; }
        public string dep_5_gender { get; set; }
        public string dep_6_gender { get; set; }
        public string dep_7_gender { get; set; }
        public string dep_8_gender { get; set; }
        public string dep_9_gender { get; set; }
        public string dep_10_gender { get; set; }
        public string dep_1_date_of_birth { get; set; }
        public string dep_2_date_of_birth { get; set; }
        public string dep_3_date_of_birth { get; set; }
        public string dep_4_date_of_birth { get; set; }
        public string dep_5_date_of_birth { get; set; }
        public string dep_6_date_of_birth { get; set; }
        public string dep_7_date_of_birth { get; set; }
        public string dep_8_date_of_birth { get; set; }
        public string dep_9_date_of_birth { get; set; }
        public string dep_10_date_of_birth { get; set; }
        public string Dep_1_Type { get; set; }
        public string Dep_2_Type { get; set; }
        public string Dep_3_Type { get; set; }
        public string Dep_4_Type { get; set; }
        public string Dep_5_Type { get; set; }
        public string Dep_6_Type { get; set; }
        public string Dep_7_Type { get; set; }
        public string Dep_8_Type { get; set; }
        public string Dep_9_Type { get; set; }
        public string Dep_10_Type { get; set; }

        public string DependantTypeCode { get; set; }
        public string Child_dependent_cost { get; set; }
        public string Adult_Dependent_Cost { get; set; }
        public string Main_Member_cost { get; set; }

       // public string race { get; set; }
        public string dep_additional_buy_up_1 { get; set; }
        public string dep_additional_buy_up_2 { get; set; }
        public string dep_additional_buy_up_3 { get; set; }
        public string dep_additional_buy_up_4 { get; set; }
        public string dep_additional_buy_up_5 { get; set; }
        public string dep_additional_buy_up_6 { get; set; }
        public string dep_additional_buy_up_7 { get; set; }
        public string dep_additional_buy_up_8 { get; set; }
        public string dep_additional_buy_up_9 { get; set; }
        public string dep_additional_buy_up_10 { get; set; }
        public string Additional_Buy_Up { get; set; }

        public string title { get; set; }
        public string firstname { get; set; }
        public string surname { get; set; }
        public string id_type { get; set; }
        public string id_number { get; set; }
        public string date_of_birth { get; set; }
       // public string gender { get; set; }
        public string Comp_Code { get; set; }
        public string mip_status { get; set; }
        public string postal_address_line_1 { get; set; }
        public string postal_address_line_2 { get; set; }
        public string suburb { get; set; }
        public string city { get; set; }
        public string postal_code { get; set; }
        public string email { get; set; }
        public string alternative_number { get; set; }
        public string cell_number { get; set; }
        public string id { get; set; }
    }


    public class MIP
    {
        public string name { get; set; }
        public string id { get; set; }
    }

    public class ZohoPutRequest
    {
        public List<ZohoPut> data { get; set; }
    }

    public class ZohoPut
    {
        public string mipresponse { get; set; }
        public string mipresponsemessage { get; set; }
        public string mip_medstat_number { get; set; }
        public string mip_policy_premium { get; set; }
        public string mip_main_member_premium { get; set; }
    }

}
