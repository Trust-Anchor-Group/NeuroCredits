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

<form action="Settings.md" method="post">
<fieldset>
<legend>Neuro-Credits™ settings</legend>

{{
if exists(Posted) then
(
	SetSetting("TAG.Payments.NeuroCredits.Countries",Posted.Countries);
	SetSetting("TAG.Payments.NeuroCredits.Currencies",Posted.Currencies);
	SetSetting("TAG.Payments.NeuroCredits.PrivatePersons",Boolean(Posted.AllowPrivatePersons));
	SetSetting("TAG.Payments.NeuroCredits.Organizations",Boolean(Posted.AllowOrganizations));
	SetSetting("TAG.Payments.NeuroCredits.DefaultPersonalLimit",Num(Posted.DefaultPersonalLimit));
	SetSetting("TAG.Payments.NeuroCredits.DefaultOrganizationalLimit",Num(Posted.DefaultOrganizationalLimit));

	TAG.Payments.NeuroCredits.ServiceConfiguration.InvalidateCurrent();

	SeeOther("Settings.md");
);
}}

<p>
<label for="Countries">Enable service for these **Countries**: ([ISO 3166-1 2-letter country codes](https://en.wikipedia.org/wiki/ISO_3166-1), separated by commas):</label>  
<input id="Countries" name="Countries" type="text" value="{{GetSetting('TAG.Payments.NeuroCredits.Countries','')}}"/>
</p>

<p>
<label for="Currencies">Enable service for these **Currencies**: ([IBAN 3-letter currency codes](https://www.iban.com/currency-codes), separated by commas):</label>  
<input id="Currencies" name="Currencies" type="text" value="{{GetSetting('TAG.Payments.NeuroCredits.Currencies','')}}"/>
</p>

<p>
<label for="DefaultPersonalLimit">Default personal credit limit: ({{DefaultCurrency:=GetSetting('DefaultCurrency','')}}):</label>  
<input id="DefaultPersonalLimit" name="DefaultPersonalLimit" type="number" min="0" value="{{GetSetting('TAG.Payments.NeuroCredits.DefaultPersonalLimit',0)}}"/>
</p>

<p>
<label for="DefaultOrganizationalLimit">Default organizational credit limit: ({{DefaultCurrency}}):</label>  
<input id="DefaultOrganizationalLimit" name="DefaultOrganizationalLimit" type="number" min="0" value="{{GetSetting('TAG.Payments.NeuroCredits.DefaultOrganizationalLimit',0)}}"/>
</p>

Who can use the service:

<p><
<input id="AllowPrivatePersons" name="AllowPrivatePersons" type="checkbox" "{{GetSetting('TAG.Payments.NeuroCredits.PrivatePersons',false)?"checked":""}}"/>
<label for="AllowPrivatePersons">Allow private persons to buy Neuro-Credits™</label>
</p>

<p>
<input id="AllowOrganizations" name="AllowOrganizations" type="checkbox" "{{GetSetting('TAG.Payments.NeuroCredits.Organizations',false)?"checked":""}}"/>
<label for="AllowOrganizations">Allow organizations to buy Neuro-Credits™</label>
</p>

<button type="submit" class="posButton">Apply</button>
</fieldset>
</form>