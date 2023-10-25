AuthenticateSession(Request,"User");
Authorize(User,"Admin.Payments.NeuroCredits");

({
	"OrganizationName":Required(Str(POrganizationName))
}:=Posted) ??? BadRequest("Invalid content in request.");

Organization:=select top 1 * from TAG.Payments.NeuroCredits.Data.OrganizationConfiguration where OrganizationName=POrganizationName;
if !exists(Organization) then NotFound("Organization not found.");

TAG.Payments.NeuroCredits.ServiceConfiguration.CurrentOrganizationDebt(Organization.OrganizationNumber,Organization.OrganizationCountry);