Title: General
Description: General settings for the Neuro-Credits™ service.
Date: 2023-10-31
Author: Peter Waher
Master: /Master.md
Javascript: Settings.js
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Payments.NeuroCredits
Login: /Login.md

========================================================================

General Neuro-Credits™ settings
==================================

Below you can edit the **default** settings of the Neuro-Credits™ service. These settings will be used if no specific individual or organizational
settings are available. You can access individual account or organizational on separate pages:

<form action="General.md" method="post">
<fieldset>
<legend>General Settings</legend>

{{
if exists(Posted) then
(
	Period:=Duration(Posted.Period) ??? error("Invalid Duration");

	SetSetting("TAG.Payments.NeuroCredits.Countries",Posted.Countries);
	SetSetting("TAG.Payments.NeuroCredits.Currencies",Posted.Currencies);
	SetSetting("TAG.Payments.NeuroCredits.PrivatePersons",Boolean(Posted.AllowPrivatePersons ??? false));
	SetSetting("TAG.Payments.NeuroCredits.Organizations",Boolean(Posted.AllowOrganizations ??? false));
	SetSetting("TAG.Payments.NeuroCredits.DefaultPersonalLimit",Num(Posted.DefaultPersonalLimit));
	SetSetting("TAG.Payments.NeuroCredits.DefaultOrganizationalLimit",Num(Posted.DefaultOrganizationalLimit));
	SetSetting("TAG.Payments.NeuroCredits.InvoiceFee",Num(Posted.InvoiceFee));
	SetSetting("TAG.Payments.NeuroCredits.Period",Str(Period));
	SetSetting("TAG.Payments.NeuroCredits.PeriodInterest",Num(Posted.PeriodInterest));
	SetSetting("TAG.Payments.NeuroCredits.MaxInstallments",Num(Posted.MaxInstallments));

	TAG.Payments.NeuroCredits.Configuration.ServiceConfiguration.InvalidateCurrent();

	SeeOther("General.md");
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
<label for="InvoiceFee">Invoice fee: ({{DefaultCurrency}})</label>  
<input id="InvoiceFee" name="InvoiceFee" type="number" min="0" value="{{GetSetting('TAG.Payments.NeuroCredits.InvoiceFee',0)}}" style="max-width:20em"/>
</p>

<p>
<label for="Period">Credit **period** (i.e. [ISO 8601 duration](https://www.digi.com/resources/documentation/digidocs//90001488-13/reference/r_iso_8601_duration_format.htm) 
until first installment, and between installments):</label>  
<input id="Period" name="Period" type="text" pattern="^(-?)P(?=\d|T\d)(?:(\d+)Y)?(?:(\d+)M)?(?:(\d+)([DW]))?(?:T(?:(\d+)H)?(?:(\d+)M)?(?:(\d+(?:\.\d+)?)S)?)?$" value="{{GetSetting('TAG.Payments.NeuroCredits.Period','P1M')}}" style="max-width:20em"/>
</p>

<p>
<label for="PeriodInterest">Period **interest**: (\%)</label>  
<input id="PeriodInterest" name="PeriodInterest" type="number" min="0" value="{{GetSetting('TAG.Payments.NeuroCredits.PeriodInterest',2)}}" style="max-width:20em"/>
</p>

<p>
<label for="MaxInstallments">Maximum number of **installments**:</label>  
<input id="MaxInstallments" name="MaxInstallments" type="number" min="1" max="12" value="{{GetSetting('TAG.Payments.NeuroCredits.MaxInstallments',6)}}" style="max-width:20em"/>
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
<button type="button" class="negButton" onclick="Close()">Close</button>
</fieldset>
</form>
