Title: Development
Description: Development settings for the Neuro-Credits™ service.
Date: 2023-11-14
Author: Peter Waher
Master: /Master.md
Javascript: Settings.js
Javascript: /Events.js
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Payments.NeuroCredits
Login: /Login.md

========================================================================

Development settings
=======================

For testing of the Neuro-Credits™ on development Neurons (i.e. neurons with an empty domain name), local contracts need to be used.
Upload the contracts `BuyNeuroCredits.xml` and `SellNeuroCredits.xml` available in the [NeuroCredits repository](https://github.com/Trust-Anchor-Group/NeuroCredits)
under thge `Contracts` folder, to the neuron, and approve the templates. Enter the Contract IDs of the corresponding templates below,
and press the `Apply` button to apply the changes. This will enable the service for approved users (see the other settings available).

<form action="Development.md" method="post">
<fieldset>
<legend>Development Settings</legend>

{{
if exists(Posted) then
(
	SetSetting("TAG.Payments.NeuroCredits.Dev.BuyEDalerTemplateId",Posted.BuyEDalerTemplateId);
	SetSetting("TAG.Payments.NeuroCredits.Dev.SellEDalerTemplateId",Posted.SellEDalerTemplateId);
	
	SeeOther("Development.md");
);
}}

<p>
<label for="BuyEDalerTemplateId">Contract ID of template for **buying eDaler(R)**:</label>  
<input id="BuyEDalerTemplateId" name="BuyEDalerTemplateId" type="text" value="{{GetSetting('TAG.Payments.NeuroCredits.Dev.BuyEDalerTemplateId','')}}"/>
</p>

<p>
<label for="SellEDalerTemplateId">Contract ID of template for **selling eDaler(R)**:</label>  
<input id="SellEDalerTemplateId" name="SellEDalerTemplateId" type="text" value="{{GetSetting('TAG.Payments.NeuroCredits.Dev.SellEDalerTemplateId','')}}"/>
</p>

<button type="submit" class="posButton">Apply</button>
<button type="button" class="negButton" onclick="Close()">Close</button>

</fieldset>
</form>
