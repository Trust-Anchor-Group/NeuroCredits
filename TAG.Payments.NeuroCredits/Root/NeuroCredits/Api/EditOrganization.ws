AuthenticateSession(Request,"User");
Authorize(User,"Admin.Payments.NeuroCredits");

({
	"ObjectId":Optional(Str(PObjectId)),
	"OrganizationName":Required(Str(POrganizationName)),
	"OrganizationNumber":Required(Str(POrganizationNumber)),
	"OrganizationCountry":Required(Str(POrganizationCountry) like "^[A-Z]{2}$"),
	"PersonalNumbers":Required(Str(PPersonalNumbers)),
	"PersonalCountries":Required(Str(PPersonalCountries)),
	"Amount":Required(Num(PAmount) >= 0),
	"Currency":Required(Str(PCurrency) like "^[A-Z]{3}$"),
	"Period":Required(Duration(PPeriod)),
	"PeriodInterest":Required(Num(PPeriodInterest) >= 0),
	"MaxInstallments":Required(1 <= Int(PMaxInstallments) <= 12)

}:=Posted) ??? BadRequest("Invalid content in request.");

if empty(POrganizationName) then BadRequest("Organization Name cannot be empty.");
if empty(POrganizationNumber) then BadRequest("Organization Number cannot be empty.");
if PPeriod <=  Duration("PT0S") then BadRequest("Period must be positive.");

if empty(PPersonalNumbers) and empty(PPersonalCountries) then
(
	PPersonalNumbers:=null;
	PPersonalCountries:=null;
)
else
(
	PPersonalNumbers:=PPersonalNumbers.Split(",",System.StringSplitOptions.None);
	PPersonalCountries:=PPersonalCountries.Split(",",System.StringSplitOptions.None);

	if count(PPersonalNumbers) != count(PPersonalCountries) then 
	(
		if count(PPersonalCountries)=1 then
			PPersonalCountries:=[foreach x in 1..count(PPersonalNumbers) do PPersonalCountries[0]]
		else
			BadRequest("You must provide the same number of personal numbers as countries");
	);

	foreach PersonalCountry in PPersonalCountries do
		if !(PersonalCountry like "^[A-Z]{2}$") then BadRequest("Invalid country code.");
);

if empty(PObjectId) then
(
	Organization:=select top 1 * from TAG.Payments.NeuroCredits.Data.OrganizationConfiguration where OrganizationName=POrganizationName;
	if exists(Organization) then Conflict("Organization settings already defined for this organization.");
	Organization:=Create(TAG.Payments.NeuroCredits.Data.OrganizationConfiguration)
)
else 
(
	Organization:=select top 1 * from TAG.Payments.NeuroCredits.Data.OrganizationConfiguration where ObjectId=PObjectId;
	if !exists(Organization) then NotFound("Organization is not found.");

	if Organization.OrganizationName != POrganizationName then
	(
		Organization2:=select top 1 * from TAG.Payments.NeuroCredits.Data.OrganizationConfiguration where Organization=POrganizationName;
		if exists(Organization2) then Conflict("Organization settings already defined for this organization.");
	)
);

Organization.OrganizationName := POrganizationName;
Organization.OrganizationNumber := POrganizationNumber;
Organization.OrganizationCountry := POrganizationCountry;
Organization.PersonalNumbers := PPersonalNumbers;
Organization.PersonalCountries := PPersonalCountries;
Organization.MaxCredit := PAmount;
Organization.Currency := PCurrency;
Organization.Period := PPeriod;
Organization.PeriodInterest := PPeriodInterest;
Organization.MaxInstallments:= PMaxInstallments;

if empty(PObjectId) then
(
	Organization.Created := Now;
	Organization.Updated := System.DateTime.MinValue;

	SaveNewObject(Organization);
	LogMessage:="Neuro-Credits™ organization settings created.";
)
else
(
	Organization.Updated := Now;
	UpdateObject(Organization);
	LogMessage:="Neuro-Credits™ organization settings updated.";
);

LogNotice(LogMessage,
{
	"Object": POrganizationName,
	"Actor": User.UserName,
	"OrganizationName": POrganizationName,
	"OrganizationNumber": POrganizationNumber,
	"OrganizationCountry": POrganizationCountry,
	"PersonalNumbers": PPersonalNumbers,
	"PersonalCountries": PPersonalCountries,
	"MaxCredit": PAmount,
	"Currency": PCurrency,
	"Period": PPeriod.ToString(),
	"PeriodInterest": PPeriodInterest,
	"MaxInstallments": PMaxInstallments
});

{
	"ObjectId": Organization.ObjectId,
	"OrganizationName": POrganizationName,
	"OrganizationNumber": POrganizationNumber,
	"OrganizationCountry": POrganizationCountry,
	"PersonalNumbers": PPersonalNumbers,
	"PersonalCountries": PPersonalCountries,
	"MaxCredit": PAmount,
	"Currency": PCurrency,
	"Period": PPeriod.ToString(),
	"PeriodInterest": PPeriodInterest,
	"MaxInstallments": PMaxInstallments
}
