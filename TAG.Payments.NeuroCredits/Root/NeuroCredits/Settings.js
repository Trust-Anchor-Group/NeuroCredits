function AddAccount()
{
	var Table = FindTable("IndividualAuthorizations");
}

function EditAccount(Control)
{
}

function DeleteAccount(Control)
{
}

function AddOrganization()
{
	var Table = FindTable("OrganizationAuthorizations");
}

function EditOrganization(Control)
{
}

function DeleteOrganization(Control)
{
}

function FindTable(ParentId)
{
	var Parent = document.getElementById(ParentId);
	return FindChild(Parent, "TABLE");
}

function FindChild(Parent, TagName)
{
	var Loop = Parent.firstChild;

	while (Loop && Loop.tagName !== TagName)
		Loop = Loop.nextSibling;

	return Loop;
}