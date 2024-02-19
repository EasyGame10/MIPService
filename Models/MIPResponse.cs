using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPService.Models
{

    public class MemberDetail
    {
        public MemberDets MemberDetails { get; set; }
        public PolicyDetails PolicyDetails { get; set; }
        public string UserId { get; set; }
        public string UserPassword { get; set; }

    }

    public class MemberDets
    {
        public string Xref { get; set; } //policy numb
        public string Title { get; set; } //tiltle
        public string FirstName { get; set; } //firstname
        public string Surname { get; set; } //surname
        public string PassportNumber { get; set; }//if id_type == sa = id_number ELSE PASS
        public string IDNumber { get; set; }//id_number
        public string DateOfBirth { get; set; } //date_of_birth
        public string Gender { get; set; }//gender
        public string Initials { get; set; }//First letter of first name
        public string EmployerName { get; set; }//Comp_Code
        public string EmploymentDate { get; set; }//dont set
        public string StatusDescription { get; set; }//mip_status
        public string ResignationDate { get; set; }//blank
        public string PostalAddress1 { get; set; }//postal_address_line_1
        public string PostalAddress2 { get; set; }//postal_address_line_2
        public string PostalAddress3 { get; set; }//suburb
        public string PostalProvince { get; set; }//city
        public string PostalPostalCode { get; set; }//postal_code
        public string PhysicalAddress1 { get; set; }//postal_address_line_1
        public string PhysicalAddress2 { get; set; }//postal_address_line_2
        public string PhysicalAddress3 { get; set; }//suburb
        public string PhysicalProvince { get; set; }//city
        public string PhysicalPostalCode { get; set; }//postal_code
        public string EmailAddress { get; set; }//email
        public string HomeTelephoneNumber { get; set; }//alternative_number
        public string WorkTelephoneNumber { get; set; }//blank
        public string CellphoneNumber { get; set; }//cell_number
        public string PastYearAdmission { get; set; }//static Yes cofig
        public string AdditionalBuyUp { get; set; }//additional_buy_up pull from MIP CustomModule1 take policy and search for D Name:equals:policy_number
        public string PROCESS_DTTM { get; set; }//static config C
        //public PolicyDetails PolicyDetails { get; set; }
    }

    public class DependantInfo
    {
        public List<DependantDet> DependantDetails { get; set; }
    }
    public class DependantDet
    {
        public string DEP { get; set; }// dep1 - dep10//total_dependants
        public string DependantInceptionDate { get; set; }//inception date all dependants 
        public string DependantFirstName { get; set; } //firstname //dep1name
        public string DependantSurname { get; set; }//surname //dep_1_surname
        public string DependantGender { get; set; }//gender //dep_1_gender {M/F}
        public string IDNumber { get; set; }//id_number //dep_1_date_of_birth
        public string PassportNumber { get; set; }//dep_1_date_of_birth
        public string DateOfBirth { get; set; }//dep_1_date_of_birth
        public string DependantRelationship { get; set; }//main //Dep_1_Type
        public string DependantTypeDescription { get; set; }//gender //dep_1_gender
        public string DependantTypeCode { get; set; }//0 main //2 -spouse - 3-adult - 4- child
        public string DependantResignationDate { get; set; }//blank
        public string PastYearAdmission { get; set; }//blank
        public string PREM { get; set; }// child - Child_dependent_cost -spouse/adult - Adult_Dependent_Cost //main - Main_Member_cost
        public string EthnicGroup { get; set; }// race
        public string AdditionalBuyUp { get; set; }//mip - dep_additional_buy_up_1 -10 //main mip - Additional_Buy_Up
        public string OID { get; set; }// 0 static config 
    }

    public class PolicyDetails
    {
        public string PolicyNumber { get; set; }//policy_number
        public string PolicyInceptionDate { get; set; }//inception_date
        public string DebitOrderDate { get; set; }//debit_order_date
        public string BrokerCode { get; set; }//Broker_Number
        public string ProductCode { get; set; }//deals module - product
        public string AccountHolder { get; set; }//account_holder_name
        public string AccountTypeCode { get; set; }//account_type -1-cheque 2 is savings
        public string AccountNumber { get; set; }//account_number
        public string BranchCode { get; set; }//Mip module branch_code_clm
        public string ClaimAccountHolder { get; set; } //account_holder_name
        public string ClaimAccountTypeCode { get; set; } //account_type -1-cheque 2 is savings
        public string ClaimAccountNumber { get; set; }//account_number
        public string ClaimBranchCode { get; set; }//Mip module branch_code_clm
        public string AccHolderID { get; set; } //blank
        public string AVSComp { get; set; }//static 0 config
        public string DOMandate { get; set; }//static 1 config
        public string MarketingAuth { get; set; }//Date and Time, created_time
        public string EthnicGroup { get; set; }//mip module - race
        public string Gender { get; set; }//gender
        public string PREM { get; set; }//Premium_Test
        public DependantInfo DependantInformation { get; set; }
    }

    public class MipResponse
    {
        public List<MipData> data { get; set; }
    }

    public class MipData
    {
        public string REQUEST_TYPE { get; set; }
        public string EMPLID { get; set; }
        public string POLICY_NBR { get; set; }
        public bool ERROR { get; set; }
        public string ERROR_MESSAGE { get; set; }
        public string REFERENCE { get; set; }
    }
}

