﻿using System;
using NHN.DtoContracts.Common.en;
using System.Collections.Generic;
using System.ServiceModel;
using NHN.DtoContracts.Flr.Data;
using NHN.DtoContracts.Htk;

namespace NHN.DtoContracts.Flr.Service
{
    /// <summary>
    /// Skriveoperasjoner til FLR. Eneste bruker er FLO p.t.
    /// </summary>
    [ServiceContract(Namespace = FlrXmlNamespace.V1)]
    public interface IFlrWriteOperations
    {
        /// <summary>
        /// Lag en mengde historiske bedrifter. Historiske bedrifter er Business-objekter med et negativt organisasjonsnummer 
        /// for å skille dem fra bedrifter med gyldige, faktiske organisasjonsnummer.
        /// De eneste feltene i det innkommende Business objektet som skal være satt er:
        /// OrganizationName, PhysicalAddreses, ElectronicAddresses, Name, DisplayName, Valid
        /// Alle andre datafelter må være null/0. 
        /// </summary>
        /// <param name="businesses">Listen over bedrifter man ønsker lage.</param>
        /// <returns>
        /// ID'er til opprettede business'es. Den returnerte arrayen mapper 1-1 til business parameteren. 
        /// Dvs business[i]'s ID vil komme i ret[i]. Dette vil være _negative_ nummer.
        /// </returns>
        /// <exception cref="ArgumentException">Kastes hvis bedriften har en ugyldig id</exception>
        /// <example>
        /// <code>
        /// flrWriteService.CreateHistoricalBusinessBulk(businessesList);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        int[] CreateHistoricalBusinessBulk(Business[] businesses);

        /// <summary>
        /// Sette en alternativ besøksadresse fra FLO enn den som er registrert i andre autorative registre
        /// </summary>
        /// <remarks>Registrere egen type av besøksadresse som er unik for FLO. Benyttes for opprettelse og oppdatering av denne type adresse.</remarks>
        /// <param name="organizationNumber">Referanse til virksomhet i Bedriftsregister</param>
        /// <param name="resFlo">Type MÅ være RES_FLO. Generisk objekt for fysisk adressetype</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis adresse typen er feil, må være RES_FLO</exception>
        /// <example>
        /// <code>
        /// //physicalAddresse.Type.CodeValue = RES_FLO
        /// flrWriteService.SetCustomFloAddress(organizationNumber, physicalAddresse);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void SetCustomFloAddress(int organizationNumber, PhysicalAddress resFlo);

        /// <summary>
        /// Registrere/oppdatere elektroniske adresse av type adressekomponenter. Kan også benyttes til sletting av adresser.
        /// </summary>
        /// <remarks>For å slette en adresse, sett alle elementer i ElektroniskeAdresser bortsett fra .Type til NULL/0.</remarks>
        /// <param name="organizationNumber">Referanse til virksomhet i Bedriftsregister</param>
        /// <param name="electronicAddresses">Liste av elektroniske kontaktmuligheter som er koblet til en virksomhet</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis listen med elektroniske adresser er tom</exception>
        /// <example>
        /// <code>
        /// //For å legge til eller endre en elektroniskadresse
        /// flrWriteService.SetElectronicAddresses(organizationNumber, electronicAddresses);
        /// 
        /// //For å slette tilhørende elektroniskeadresse. Sett kun type og alt annet til null på den som skal slettes
        /// // electronicAddresses.Type.CodeValue = ElectronicAddressType.Telephone
        /// // electronicAddresses.Address = null
        /// flrWriteService.SetElectronicAddresses(organizationNumber, electronicAddresses);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void SetElectronicAddresses(int organizationNumber,  ICollection<ElectronicAddress> electronicAddresses);

        /// <summary>
        /// Slette overflødig besøksadresse (RES_FLO) fra virksomheten.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="organizationNumber">Referanse til virksomhet i Bedriftsregister</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes når en organisasjonsenhet med angitt organisasjonsnummer ikke har en besøksadresse for fastlegeordningen</exception>
        /// <example>
        /// <code>
        /// flrWriteService.DeleteCustomFloAddressOnGPOffice(organizationNumber);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void DeleteCustomFloAddressOnGPOffice(int organizationNumber);

        /// <summary>
        /// Opprette en ny fastlegeavtale slik at ny fastlegeliste med relevante attributter blir etablert i registerplattform.
        /// </summary>
        /// <remarks>
        /// Opprette avtalen mellom en lege og kommune.
        /// Det forutsettes at lege finnes allerede fra før i HPR og at legen er tilknyttet en legekontor(TreatmentCenter) som finnes i Adresseregisteret/Bedriftsregisteret.
        /// 
        /// Publiserer event "ContractCreated" ved vellykket operasjon.
        /// </remarks>
        /// <param name="newGPContract">En ny legekontrakt</param>
        /// <exception cref="ArgumentException">Kastes hvis legeperiode i kontrakten eksisterer men har en ugyldig id</exception>
        /// <example>
        /// <code>
        /// flrWriteService.CreateGPContract(newGPContract);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CreateGPContract(GPContract newGPContract);

        /// <summary>
        /// Oppretter en ny fastlegeavtale - importversjon (se CreateGPContract)
        /// </summary>
        /// <remarks>Publiserer ikke events.</remarks>
        /// <seealso cref="CreateGPContract"/>
        /// <param name="bulkGPContracts"></param>
        /// /// <exception cref="ArgumentException">Kastes hvis legeperiode i kontrakten eksisterer men har en ugyldig id</exception>
        /// <example>
        /// <code>
        /// flrWriteService.CreateGPContract(listOfContracts);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CreateGPContractBulk(List<GPContract> bulkGPContracts);

        /// <summary>
        /// Oppdatere en eksisterende fastlegeavtale med nye opplysninger slik at fastlegeliste i registerplattform får oppdatert registrerte verdier
        /// </summary>
        /// <remarks>
        /// Benyttes for oppdatering/endring/avslutning av en eksisterende fastlegeavtale
        /// 
        /// Publiserer event "ContractUpdated" ved vellykket operasjon.
        /// </remarks>
        /// <param name="gpContract">En eksisterende legekontrakt, som skal oppdateres</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis id på en avtale ikke er høyere enn 0</exception>
        /// <exception cref="ArgumentException">Kastes hvis avtalen ikke finnes</exception>
        /// <example>
        /// <code>
        /// flrWriteService.UpdateGPContract(gpContract);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void UpdateGPContract(GPContract gpContract);


        /// <summary>
        /// Oppdatering av listetak på en fastlegeavtale uten at andre verdier skal endre seg.
        /// </summary>
        /// <remarks>Publiserer event "ContractUpdated" ved vellykket operasjon.</remarks>
        /// <param name="gpContractId">Id på fastlegeavtalen</param>
        /// <param name="maxPatients">Listetak på en avtale</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis listetaket på en avtale ikke er høyere enn 0</exception>
        /// <example>
        /// <code>
        /// flrWriteService.GetPatientGPDetails(gpContractId, maxPatients);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void UpdateGPContractMaxPatients(long gpContractId, int maxPatients);

        /// <summary>
        /// Oppdatering av listestatus uten endringer av andre verdier.
        /// </summary>
        /// <remarks>
        /// Kun endring til statusene Åpne og Lukke.
        /// 
        /// Publiserer event "ContractUpdated" ved vellykket operasjon.
        /// </remarks>
        /// <param name="gpContractId">Id på fastlegeavtalen</param>
        /// <param name="status">
        /// Status på liste status med referanse til kodeverk.
        /// Kodeverk: <see href="/CodeAdmin/EditCodesInGroup/flrv2_statuscode">flrv2_statuscode</see> (OID 7751).
        /// </param>
        /// <value></value>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis avtalen ikke finnes</exception>
        /// <example>
        /// <code>
        /// flrWriteService.GetPatientGPDetails(gpContractId, status);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void UpdateGPContractStatus(long gpContractId, Code status); //Kun tillate Åpne, Lukke 


        // --------------------------
        // Utekontor 
        // --------------------------

        /// <summary>
        /// Oppretter et utekontor registrert på overordnet fastlegepraksis/avtale.
        /// </summary>
        /// <remarks>
        /// Hvis et fastlegekontor har dislokerte behandlingssteder (utekontorer) så skal det kunne registreres på overordnet fastlegepraksis/avtale.
        /// 
        /// Publiserer event "OutOfOfficeLocationCreated" ved vellykket operasjon.
        /// </remarks>
        /// <param name="office">Utekontordata</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis enkontrakts id er ugyldig</exception>
        /// <exception cref="ArgumentException">Kastes hvis et utekontor med samme id finnes</exception>
        /// <example>
        /// <code>
        /// flrWriteService.CreateOutOfOfficeLocation(office);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CreateOutOfOfficeLocation(OutOfOfficeLocation office);

        /// <summary>
        /// Oppdatererer et utekontor.
        /// </summary>
        /// <remarks>
        /// Oppdatering av opplysninger om et utekontor.
        /// 
        /// Publiserer event "OutOfOfficeLocationUpdated" ved vellykket operasjon.
        /// </remarks>
        /// <param name="office">Eksisterende utekontordata</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis avtalen ikke finnes</exception>
        /// <exception cref="ArgumentException">Kastes hvis legekontoret ikke finnes</exception>
        /// <example>
        /// <code>
        /// flrWriteService.UpdateOutOfOfficeLocation(office);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void UpdateOutOfOfficeLocation(OutOfOfficeLocation office);

        /// <summary>
        /// Fjerner et utekontor. Dette er det samme som å sette utekontoret til utløpt.
        /// </summary>
        /// <remarks>
        /// Sletter et utekontor fra liste over utekontorer.
        /// 
        /// Publiserer event "OutOfOfficeLocationDeleted" ved vellykket operasjon.
        /// </remarks>
        /// <param name="outOfOfficeId">Id til utekontoret som skal slettes</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis et legekontor ikke har besøksadresse for fastlegeordningen</exception>
        /// <example>
        /// <code>
        /// flrWriteService.RemoveOutOfOfficeLocation(outOfOfficeId);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void RemoveOutOfOfficeLocation(long outOfOfficeId);


        // --------------------------
        // Listetilhørighet 
        // --------------------------

        /// <summary>
        /// Oppretter en kontraktsperiode for en lege på en fastlegeavtale.
        /// </summary>
        /// <remarks>
        /// Lager en lenke mellom lege i bestemt rolle til en fastlegeavtale.
        /// 
        /// Publiserer event "GPOnContractCreated" ved vellykket operasjon.
        ///  </remarks>
        /// <param name="association">Koblingen for en periode legen er tilknyttet en fastlegeavtale</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis perioden ikke er gyldig</exception>
        /// <exception cref="ArgumentException">Kastes hvis hpr nummeret er ugyldig</exception>
        /// <exception cref="ArgumentException">Kastes hvis kontrakten ikke finnes</exception>
        /// <example>
        /// <code>
        /// flrWriteService.CreateGPOnContractAssociation(association);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CreateGPOnContractAssociation(GPOnContractAssociation association);


        /// <summary>
        /// Oppretter en kontraktsperiode for en lege på en GPContract - importversjon.
        /// </summary>
        /// <remarks>Publiserer ikke events.</remarks>
        /// <seealso cref="CreateGPOnContractAssociation"/>
        /// <remarks>Se CreateGPOnContractAssociation(GPOnContractAssociation association. Tar i mot en liste av koblinger.</remarks>
        /// <param name="creates"></param>
        /// <exception cref="ArgumentException">Kastes hvis perioden ikke er gyldig</exception>
        /// <exception cref="ArgumentException">Kastes hvis hpr nummeret er ugyldig</exception>
        /// <exception cref="ArgumentException">Kastes hvis kontrakten ikke finnes</exception>
        /// <example>
        /// <code>
        /// flrWriteService.CreateGPOnContractAssociationBulk(ContractAssociationList);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CreateGPOnContractAssociationBulk(List<GPOnContractAssociation> creates);

        /// <summary>
        /// Oppdatere eksisterende kobling mellom leger og eksisterende avtaler.
        /// </summary>
        /// <remarks>
        /// Oppdaterer informasjonen mellom lege i bestemt rolle til en fastlegeavtale.
        /// 
        /// Publiserer event "GPOnContractUpdated" ved vellykket operasjon.
        /// </remarks>
        /// <param name="association">Eksisterende koblingen for en periode legen er tilknyttet en fastlegeavtale</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis fastlegeavtalen med den angitte id ikke finnes</exception>
        /// <example>
        /// <code>
        /// flrWriteService.UpdateGPOnContractAssociation(association);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void UpdateGPOnContractAssociation(GPOnContractAssociation association);


        /// <summary>
        /// Sletter en kontraktsperiode for en lege på en GPContract.
        /// </summary>
        /// <remarks>
        /// Publiserer event "GPOnContractDeleted" ved vellykket operasjon.
        /// </remarks>
        /// <param name="gpOnContractAssociationId"></param>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void DeleteGPOnContractAssociation(long gpOnContractAssociationId);
        // --------------------------
        // LegeSprak 
        // --------------------------

        /// <summary>
        /// Oppdaterer listen over språk en gitt lege kan snakke.
        /// </summary>
        /// <remarks>Registrerer språk på helsepersonell. En tom liste sletter alle språk på angitt helsepersonell.</remarks>
        /// <param name="hprNumber">Referanse id til helsepersonell</param>
        /// <param name="languages">Liste av språk med referanse til kodeverk.
        /// Kodeverk: <see href="/CodeAdmin/EditCodesInGroup/sprak">sprak</see> (OID 3303), <see href="/CodeAdmin/EditCodesInGroup/norsksprak">norsksprak</see> (OID 3301).
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis listen med språk er tom</exception>
        /// <example>
        /// <code>
        /// //For å sette språk
        /// flrWriteService.GetPatientGPDetails(UpdateGPLanguages, languages);
        /// 
        /// //For å slette alle registrerte språk
        /// flrWriteService.UpdateGPLanguages(hprNumber, emptyLanguagesList);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void UpdateGPLanguages(int hprNumber, ICollection<Code> languages);


        // --------------------------
        // ListeTilhorighet
        // --------------------------

        /// <summary>
        /// Kobler en pasient til en fastlegeliste.
        /// </summary>
        /// <remarks>
        /// Opprette nyregistrert person i PREG til en eksisterende fastlegeavtale i FLR.
        /// 
        /// Publiserer event "PatientOnContractCreated" ved vellykket operasjon.
        /// </remarks>
        /// <param name="patientToGPContractAssociation">Kobling mellom innbygger og fastlegeavtale</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis pasientens nin mangler</exception>
        /// <exception cref="ArgumentException">Kastes hvis perioden ikke er gyldig</exception>
        /// <exception cref="ArgumentException">Kastes hvis hpr nummeret er ugyldig</exception>
        /// <exception cref="ArgumentException">Kastes hvis kontrakten ikke finnes</exception>
        /// <exception cref="ArgumentException">Kastes hvis perioder er overlappende</exception>
        /// <example>
        /// <code>
        /// flrWriteService.CreatePatientToGPContractAssociation(patientToGPContractAssociation);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CreatePatientToGPContractAssociation(PatientToGPContractAssociation patientToGPContractAssociation);

        /// <summary>
        /// Kobler en pasient til en fastlegeliste - importversjon
        /// </summary>
        /// <remarks>Publiserer ikke events.</remarks>
        /// <seealso cref="CreatePatientToGPContractAssociation"/>
        /// <remarks>Se CreatePatientToGPContractAssociation. Tar i mot en liste med koblinger for bulk operasjoner</remarks>
        /// <param name="patientToGPContractAssociation"></param>
        /// /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis pasientens nin mangler</exception>
        /// <exception cref="ArgumentException">Kastes hvis perioden ikke er gyldig</exception>
        /// <exception cref="ArgumentException">Kastes hvis hpr nummeret er ugyldig</exception>
        /// <exception cref="ArgumentException">Kastes hvis kontrakten ikke finnes</exception>
        /// <exception cref="ArgumentException">Kastes hvis perioder er overlappende</exception>
        /// <example>
        /// <code>
        /// flrWriteService.CreatePatientToGPContractAssociationBulk(patientToGPContractAssociationList);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CreatePatientToGPContractAssociationBulk(List<PatientToGPContractAssociation> patientToGPContractAssociation);

        /// <summary>
        ///  Flytter en innbyger fra en liste til en annen.
        /// </summary>
        /// <remarks>
        /// Flytte pasienter mellom to fastlegelister. Fødselsnummer valideres. Feiler en pasient så kastes exception på alt.
        /// 
        /// Publiserer event "PatientOnContractCreated" for hver pasient som blir flyttet.
        /// </remarks>
        /// <param name="fromGPContract">ID til fastlegeliste en innbygger skal flyttes FRA.</param>
        /// <param name="patientsToMove">Liste over innbyggere på eksisterende fastlegelister som skal flyttes</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis kontraktid er ugyldig</exception>
        /// <exception cref="ArgumentException">Kastes hvis listen er tom</exception> 
        /// <exception cref="ArgumentException">Kastes hvis en pasient ikke finnes på fastlegelisten i den gitte perioden</exception> 
        /// <exception cref="ArgumentException">Kastes hvis en pasient allerede finnes på destinasjonskontrakten</exception> 
        /// <example>
        /// <code>
        /// flrWriteService.MovePatients(fromGPContract, patientsToMove);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void MovePatients(long fromGPContract, ICollection<PatientToGPContractAssociation> patientsToMove);

        /// <summary>
        /// Avslutter fastlegeavtale og flytter innbyggere.
        /// </summary>
        /// <remarks>
        /// Flytte alle pasienter mellom listene og deretter avslutter listen hvor innbyggere ble flyttet fra.
        /// 
        /// Publiserer event "ContractCanceled" samt "PatientOnContractCreated" for hver pasient som blir flyttet
        /// ved vellykket operasjon.
        /// </remarks>
        /// <param name="gpContractId">Referanse til fastlegeliste som skal avsluttes</param>
        /// <param name="endReason">
        ///     Referanse til kode for avsluttet status.
        ///     Kodeverk: <see href="/CodeAdmin/EditCodesInGroup/flrv2_contract_endreason">flrv2_contract_endreason</see> (OID 7753).
        /// </param>
        /// <param name="period">Sluttdato på kontrakt</param>
        /// <param name="capitaToMove">Liste over innbyggere som skal flyttes til ny liste</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes hvis en pasient ikke finnes på fastlegelisten i den gitte perioden</exception>
        /// <example>
        /// <code>
        /// flrWriteService.CancelGPContractAndMovePatients(gpContractId, endReason, period, capitaToMove);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CancelGPContractAndMovePatients(long gpContractId, Code endReason, Period period, ICollection<PatientToGPContractAssociation> capitaToMove);

        /// <summary>
        /// Avslutte innbyggerens tilhørighet på en fastlegeliste/avtale. Denne metoden avslutter også alle fremtidige tilhørigheter for innbyggeren på denne kontrakten. Dvs, assiosasjoner hvor Valid.To > DateTime.Now.
        /// </summary>
        /// <remarks>
        /// Publiserer event "PatientOnContractCanceled" ved vellykket operasjon.
        /// </remarks>
        /// <param name="gpContractId">Referanse til fastlegelisten</param>
        /// <param name="patientNin">Referanse til innbyggerens fødselsnummer (eller D-nummer)</param>
        /// <param name="endReason">
        ///     Referanse til kodeverk for avslutningskode.
        ///     Kodeverk: Kodeverk: <see href="/CodeAdmin/EditCodesInGroup/flrv2_endreason">flrv2_endreason</see> (OID 7753).
        /// </param>
        /// <param name="endTime">På hvilket tidspunkt skal kontrakten avsluttes.</param>
        /// <exception cref="ArgumentException">Kastes hvis en pasient ikke finnes på fastlegelisten i den gitte perioden</exception>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CancelPatientOnGPContract(long gpContractId, string patientNin, Code endReason, DateTime endTime);

        /// <summary>
        /// Brukes for å sette visningsnavnet på et legekontor. Merk at legekontoret selv kan overskrive det som eventuelt settes her selv.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="organizationNumber">Organisasjonsnummer til legekontoret</param>
        /// <param name="displayName">Visningsnavnet man ønsker sette. Maks 150 tegn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Kastes når et organisasjonsnummer ikke er høyere enn 0</exception>
        /// <example>
        /// <code>
        /// flrWriteService.SetDisplayNameOnGPOffice(organizationNumber, displayName);
        /// </code>
        /// </example>
        [OperationContract]
        [FaultContract(typeof (GenericFault))]
        void SetDisplayNameOnGPOffice(int organizationNumber, string displayName);

        /// <summary>
        /// Oppdaterer alle referanser til en pasient som har byttet personnummer. (I praksis, alle <see cref="PatientToGPContractAssociation"/> objekter).
        /// </summary>
        /// <param name="oldNin">Gammelt personnummer</param>
        /// <param name="newNin">Nytt personnummer</param>
        /// <exception cref="ArgumentException">Hvis newNin/oldNin er ugyldige, eller det ikke finnes noen personer i FLR med personnummer oldNin</exception>
        /// <exception cref="ArgumentException">Hvis newNin ikke finnes i personregisteret</exception>
        /// <remarks>Service bus: Denne metoden sender event PatientNinChanged med property NewNin og OldNin. Den sender også PatientOnContractUpdated per <see cref="PatientToGPContractAssociation"/> som oppdateres.
        /// </remarks>
        [OperationContract]
        [FaultContract(typeof (GenericFault))]
        void UpdatePatientNin(string oldNin, string newNin);

        /// <summary>
        /// Oppdaterer GPOfficeOrganizationNumber på alle GpContracts.
        /// </summary>
        /// <param name="oldOrganizationNumber">Orgnummeret til det gamle kontoret</param>
        /// <param name="newOrganizationNumber">Orgnummeret til det nye kontoret</param>
        [OperationContract]
        [FaultContract(typeof (GenericFault))]
        void UpdateGPOfficeOnGPContracts(int oldOrganizationNumber, int newOrganizationNumber);

        /// <summary>
        /// Forsikre om at angitte personer finnes i Cache, legger til de som mangler.
        /// Dette er en slags initialisering av PersonServiceCache, for å spare tid ved Export og andre metoder som trenger personopplysninger
        /// </summary>
        /// <param name="nins">Alle Personnr til personer som skal hentes fra PersonService og inn i PersonServiceCache</param>
        [OperationContract]
        [FaultContract(typeof (GenericFault))]
        void EnsurePeopleInCache(IList<string> nins);

        #region Cleanup methods
        /// <summary>
        /// Sletter avtale, med ALLE relaterte relasjoner (Legeperioder, Tilhørigheter, Utekontor).
        /// Kun tilgjengelig i testmiljø.
        /// </summary>
        /// <param name="gpContractId">Id til fastlegeavtale.</param>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CleanupGPContract(long gpContractId);

        /// <summary>
        /// Sletter legekontorets avtaler, med ALLE relaterte relasjoner (Legeperioder, Tilhørigheter, Utekontor).
        /// Kun tilgjengelig i testmiljø.
        /// </summary>
        /// <param name="orgNr">Organisasjonsnummer til legekontoret.</param>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CleanupGPContractByOrgNr(int orgNr);

        /// <summary>
        /// Sletter legeperiode. 
        /// Kun tilgjengelig i testmiljø.
        /// </summary>
        /// <param name="gpOnContractAssociationId">Id til legeperiode.</param>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CleanupGPOnContractAssociation(long gpOnContractAssociationId);

        /// <summary>
        /// Sletter ALT om legen - Legeperioder, legespråk.
        /// Kun tilgjengelig i testmiljø.
        /// </summary>
        /// <param name="doctorHprNumber">HPR-nummer til legen.</param>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CleanupGP(int doctorHprNumber);

        /// <summary>
        /// Sletter legespråk.
        /// Kun tilgjengelig i testmiljø.
        /// </summary>
        /// <param name="doctorHprNumber">HPR-nummer til legen.</param>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CleanupGPLanguages(int doctorHprNumber);

        /// <summary>
        /// Sletter utekontor.
        /// Kun tilgjengelig i testmiljø.
        /// </summary>
        /// <param name="outOfOfficeId">Id til utekontor.</param>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CleanupOutOfOffice(long outOfOfficeId);

        /// <summary>
        /// Sletter tilhørighet.
        /// Kun tilgjengelig i testmiljø.
        /// </summary>
        /// <param name="patientToGPContractAssociation">Id til tilhørighet.</param>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CleanupPatientToGPContractAssociation(long patientToGPContractAssociation);

        /// <summary>
        /// Sletter samtlige entiteter i FLR innenfor et id-range med tilhørende relasjoner om de måtte treffe.
        /// Kun tilgjengelig i testmiljø.
        /// </summary>
        /// <param name="fromAndWithId">Fra og med Id.</param>
        /// <param name="toAndWithId">Til og med Id.</param>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CleanupByIdSeed(long fromAndWithId, long toAndWithId);

        /// <summary>
        /// Sletter alt.
        /// Kun tilgjengelig i testmiljø.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        void CleanupEverything();
        #endregion
    }
}
