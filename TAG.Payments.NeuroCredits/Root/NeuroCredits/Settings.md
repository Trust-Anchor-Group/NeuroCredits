Title: Neuro-Credits™ settings
Description: Configures the Neuro-Credits™ service.
Date: 2023-10-19
Author: Peter Waher
Master: /Master.md
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Payments.NeuroCredits
Login: /Login.md

========================================================================

<fieldset>
<legend>General settings</legend>
<form action="Settings.md" method="post">

{{
if exists(Posted) then
(
	SetSetting("TAG.Payments.NeuroCredits.Countries",Posted.Countries);
	SetSetting("TAG.Payments.NeuroCredits.Currencies",Posted.Currencies);
	SetSetting("TAG.Payments.NeuroCredits.PrivatePersons",Boolean(Posted.AllowPrivatePersons ??? false));
	SetSetting("TAG.Payments.NeuroCredits.Organizations",Boolean(Posted.AllowOrganizations ??? false));
	SetSetting("TAG.Payments.NeuroCredits.DefaultPersonalLimit",Num(Posted.DefaultPersonalLimit));
	SetSetting("TAG.Payments.NeuroCredits.DefaultOrganizationalLimit",Num(Posted.DefaultOrganizationalLimit));
	SetSetting("TAG.Payments.NeuroCredits.DueDays",Num(Posted.DueDays));
	SetSetting("TAG.Payments.NeuroCredits.DueInterest",Num(Posted.DueInterest));

	TAG.Payments.NeuroCredits.ServiceConfiguration.InvalidateCurrent();

	SeeOther("Settings.md");
);
}}

<p>
<label for="Countries">Enable service for these **Countries**: ([ISO 3166-1 2-letter country codes](https://en.wikipedia.org/wiki/ISO_3166-1), separated by commas)</label>  
<input id="Countries" name="Countries" type="text" value="{{GetSetting('TAG.Payments.NeuroCredits.Countries','')}}"/>
</p>

<p>
<label for="Currencies">Enable service for these **Currencies**: ([IBAN 3-letter currency codes](https://www.iban.com/currency-codes), separated by commas)</label>  
<input id="Currencies" name="Currencies" type="text" value="{{GetSetting('TAG.Payments.NeuroCredits.Currencies','')}}"/>
</p>

<p>
<label for="DefaultPersonalLimit">Default **personal** credit limit: ({{DefaultCurrency:=GetSetting('DefaultCurrency','')}})</label>  
<input id="DefaultPersonalLimit" name="DefaultPersonalLimit" type="number" min="0" value="{{GetSetting('TAG.Payments.NeuroCredits.DefaultPersonalLimit',0)}}" style="max-width:20em"/>
</p>

<p>
<label for="DefaultOrganizationalLimit">Default **organizational** credit limit: ({{DefaultCurrency}})</label>  
<input id="DefaultOrganizationalLimit" name="DefaultOrganizationalLimit" type="number" min="0" value="{{GetSetting('TAG.Payments.NeuroCredits.DefaultOrganizationalLimit',0)}}" style="max-width:20em"/>
</p>

<p>
<label for="DueDays">Number of **days** credit:</label>  
<input id="DueDays" name="DueDays" type="number" min="1" value="{{GetSetting('TAG.Payments.NeuroCredits.DueDays',30)}}" style="max-width:20em"/>
</p>

<p>
<label for="DueInterest">Due **interest**: (\%)</label>  
<input id="DueInterest" name="DueInterest" type="number" min="0" value="{{GetSetting('TAG.Payments.NeuroCredits.DueInterest',10)}}" style="max-width:20em"/>
</p>

Who can use the service:

<p>
<input id="AllowPrivatePersons" name="AllowPrivatePersons" type="checkbox" {{GetSetting('TAG.Payments.NeuroCredits.PrivatePersons',false)?"checked":""}}/>
<label for="AllowPrivatePersons">Allow **private persons** to buy Neuro-Credits™</label>
</p>

<p>
<input id="AllowOrganizations" name="AllowOrganizations" type="checkbox" {{GetSetting('TAG.Payments.NeuroCredits.Organizations',false)?"checked":""}}/>
<label for="AllowOrganizations">Allow **organizations** to buy Neuro-Credits™</label>
</p>

<button type="submit" class="posButton">Apply</button>
</form>
</fieldset>

<fieldset id="IndividualAuthorizations">
<legend>Individual authorizations</legend>

| Account | Personal Number | Country | Amount | Currency |   |   |
|:--------|:----------------|:--------|-------:|:---------|:-:|:-:|
{{
RS:=select Key, Value from "Settings" where Key like "TAG.Payments.NeuroCredits.Accounts.%";
foreach x in RS do
(
	if x[0] like "TAG\.Payments\.NeuroCredits\.Accounts\.(?'Account'[^\\.]+)\.(?'Country'[^\\.]+)\.(?'PNr'[^\\.]+)" then
		]]| `((Account))` | ((PNr)) | ((Country)) | ((X[1])) | ((TAG.Payments.NeuroCredits.ServiceConfiguration.GetCurrencyOfAccount(Account);)) | <button type="button" class="posButtonSm" data-account="((Account))" onclick="EditAccount(this)">Edit</button> | <button type="button" class="negButtonSm" data-account="((Account))" onclick="DeleteAccount(this)">Delete</button> |
[[
)
}}

<button type="button" class="posButton" onclick="AddAccount()">Add Account</button>

</fieldset>

<fieldset id="OrganizationAuthorizations">
<legend>Organizational authorizations</legend>

| Organization Number | Country | Personal Number | Country | Amount | Currency |   |   |
|:--------------------|:--------|:----------------|:--------|-------:|:---------|:-:|:-:|
{{
RS:=select Key, Value from "Settings" where Key like "TAG.Payments.NeuroCredits.Organization.%";
foreach x in RS do
(
	if x[0] like "TAG\.Payments\.NeuroCredits\.Organization\.(?'OrgCountry'[^\\.]+)\.(?'OrgNr'[^\\.]+)\.(?'Country'[^\\.]+)\.(?'PNr'[^\\.]+)" then
		]]| ((OrgNr)) | ((OrgCountry)) | ((PNr)) | ((Country)) | ((X[1])) | ((TAG.Payments.NeuroCredits.ServiceConfiguration.DefaultCurrency();)) | <button type="button" class="posButtonSm" data-orgnr="((OrgNr))" onclick="EditOrganization(this)">Edit</button> | <button type="button" class="negButtonSm" data-orgnr="((OrgNr))" onclick="DeleteOrganization(this)">Delete</button> |
[[
)
}}

<button type="button" class="posButton" onclick="AddOrganization()">Add Organization</button>

</fieldset>
