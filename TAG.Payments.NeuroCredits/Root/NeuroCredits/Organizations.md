Title: Organizations
Description: Configures organization-specific settings for the Neuro-Credits™ service.
Date: 2023-10-25
Author: Peter Waher
Master: /Master.md
Javascript: Settings.js
Javascript: /Events.js
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Payments.NeuroCredits
Login: /Login.md

========================================================================

Organizational Settings
===========================

Below you can edit **organizational** settings for the Neuro-Credits™ service. These settings will be used for the corresponding organizations,
instead of the default settings provided.

<form action="Organizations.md" method="post">
<fieldset id="OrganizationAuthorizations">
<legend>Organizations</legend>

| Org.Name | Org.Number | Org.Country | Personal Numbers | Countries | Amount | Currency | Period | Interest | Max Installments | Debt |   |   |
|:---------|:-----------|:------------|:-----------------|:----------|-------:|:---------|-------:|---------:|-----------------:|-----:|:-:|:-:|
{{
Organizations:=select * from TAG.Payments.NeuroCredits.Data.OrganizationConfiguration order by OrganizationName;
foreach Organization in Organizations do
(
	]]| ((Organization.OrganizationName)) [[;
	]]| ((Organization.OrganizationNumber)) [[;
	]]| ((Organization.OrganizationCountry)) [[;
	]]| ((concat(Organization.PersonalNumbers,",");)) [[;
	]]| ((concat(Organization.PersonalCountries,",");)) [[;
	]]| ((Organization.MaxCredit)) [[;
	]]| ((Organization.Currency)) [[;
	]]| ((Organization.Period)) [[;
	]]| ((Account.PeriodInterest)) % [[;
	]]| ((Organization.MaxInstallments)) [[;
	]]| ((TAG.Payments.NeuroCredits.Configuration.ServiceConfiguration.CurrentOrganizationDebt(Organization.OrganizationNumber,Organization.OrganizationCountry);)) [[;
	]]| <button type="button" class="posButtonSm" data-objectid="((Organization.ObjectId))" onclick="EditOrganization(this)">Edit</button> [[;
	]]| <button type="button" class="negButtonSm" data-objectid="((Organization.ObjectId))" onclick="DeleteOrganization(this)">Delete</button> |[[;
	]]
[[
)
}}

<input id="DefaultOrganizationalLimit" type="hidden" value="{{GetSetting('TAG.Payments.NeuroCredits.DefaultOrganizationalLimit',0)}}"/>
<input id="DefaultCurrency" type="hidden" value="{{GetSetting('DefaultCurrency','')}}"/>
<input id="DefaultPeriod" type="hidden" value="{{GetSetting('TAG.Payments.NeuroCredits.Period','P1M')}}"/>
<input id="DefaultPeriodInterest" type="hidden" value="{{GetSetting('TAG.Payments.NeuroCredits.PeriodInterest',2)}}"/>
<input id="DefaultMaxInstallments" type="hidden" value="{{GetSetting('TAG.Payments.NeuroCredits.MaxInstallments',6)}}"/>

<button type="button" class="posButton" onclick="AddOrganization()">Add Organization</button>
<button type="button" class="negButton" onclick="Close()">Close</button>

</fieldset>
</form>
