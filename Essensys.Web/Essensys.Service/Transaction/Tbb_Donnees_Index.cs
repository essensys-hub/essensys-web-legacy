using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essensys.Service.Transaction
{
    // Version #2.1

    // table de données applicatives
    // table d'échange avec l'écran / table d'échange avec les boitiers / table d'échange Ethernet

    // légende
    // BP : boitier principal
    // BA : boitier auxiliaire
    // PDV : pièces de vie
    // PDE : pièces d'eau
    // CHB : chambres
    // IHM : interface homme machine (l'écran)

    enum Constantes
    {
        // mode automatique : programmation horaire sur 1 semaine
        // 1 ordre pour 1 heure sur 7 jours : 6 modes possibles (4 bits) (cf consigne mode de chauffage)
        // soit 84 octets 
        uc_PLANNING_CHAUFFAGE_TAILLE=84,

        // arrosage 1 ordre pour 30 minutes sur 7 jours : 2 modes possibles (1 bit), soit 42 octets
        uc_PLANNING_ARROSAGE_TAILLE=42,

        // nb variateurs possibles par boitier auxiliaire
        uc_NB_VARIATEURS_POSSIBLES_PAR_BA=8,

        // nb lampes possibles par boitier auxiliaure
        uc_NB_LAMPES_POSSIBLES_PAR_BA=16,

        // nb volets possibles par boitier auxiliaire
        uc_NB_VOLETS_POSSIBLES_PAR_BA=8
    }

    // Définition des paramètres de l'alarme
    enum enumAlarmeConfig
    {
	    AlarmeConfig_Code,					// 1 = demander le code de l'alarme pour la mettre en route (0 : mise en route alarme sans code) - Géré par écran
	    AlarmeConfig_Detect1,				// détecteur de présence 1 désactivé (0) ou utilisé (1)
	    AlarmeConfig_Detect2,				// détecteur de présence 2 désactivé (0) ou utilisé (1)
	    AlarmeConfig_DetectOuv,				// détecteur d'ouverture désactivé (0) ou utilisé (1)
	    AlarmeConfig_Detect1SurVoieAcces,	// détecteur de présence 1 pas sur voie d'accès (0) ou sur voie d'accès (1)
	    AlarmeConfig_Detect2SurVoieAcces,	// détecteur de présence 2 pas sur voie d'accès (0) ou sur voie d'accès (1)
	    AlarmeConfig_DetectOuvSurVoieAcces,	// détecteur d'ouverture pas sur voie d'accès (0) ou sur voie d'accès (1)
	    AlarmeConfig_SireneInt,				// 0= ne rien faire, 1= sirène intérieure activée
	    AlarmeConfig_SireneExt,				// 0= ne rien faire, 1= sirène extérieure activée
	    AlarmeConfig_BloqueVolets,			// 0= ne rien faire, 1 = Volets roulants bloqués en cas d'alarme
	    AlarmeConfig_ForcerEclairage,		// 0= ne rien faire, 1 = Forcer allumage de toutes les éclairages
	
	    AlarmeConfig_NB_VALEURS
    };

    // Définition des paramètres pour un scénario
    enum enumScenario
    {
	    Scenario_Confirme_Scenario,		// 1 = demander confirmation - Géré par écran
	
	    // alarme
	    Scenario_Alarme_ON,			// 0 = ne rien faire, 1= mettre l'alarme, 2 = enlever l'alarme - Géré par écran
	    Scenario_AlarmeConfig,		// -> voir enumAlarmeConfig

	    // éclairage
	    Scenario_Eteindre_PDV_LSB = Scenario_AlarmeConfig + enumAlarmeConfig.AlarmeConfig_NB_VALEURS,
							    // 0  = ne rien faire / combinaison des valeurs suivantes pour éteindre les lampes voulues
							    // 1  = éteindre la lampe de l'entrée
							    // 2  = éteindre la lampe du salon
							    // 4  = éteindre la lampe du salon
							    // 8  = éteindre la lampe du dressing
							    // 16 = éteindre la lampe du dressing
	    Scenario_Eteindre_PDV_MSB,	// 0  = ne rien faire / combinaison des valeurs suivantes pour éteindre les lampes voulues 
							    // 32 = éteindre le variateur du bureau	//xxx 32 ou 1 ???
							    // 64 = éteindre le variateur de la salle à manger
							    // 128= éteindre le variateur du salon
	    Scenario_Eteindre_CHB_LSB,	// 0  = ne rien faire / combinaison des valeurs suivantes pour éteindre les lampes voulues
							    // 1  = éteindre la lampe de l'escalier
							    // 2  = éteindre la lampe de la grande chambre
							    // 4  = éteindre la lampe de la grande chambre
							    // 8  = éteindre la lampe de la petite chambre 1
							    // 16 = éteindre la lampe de la petite chambre 1
							    // 32 = éteindre la lampe de la petite chambre 2
							    // 64 = éteindre la lampe de la petite chambre 3
	    Scenario_Eteindre_CHB_MSB,	// 0= ne rien faire / combinaison des valeurs suivantes pour éteindre les lampes voulues
							    // 16 = éteindre le variateur de la petite chambre 3
							    // 32 = éteindre le variateur de la petite chambre 2
							    // 64 = éteindre le variateur de la petite chambre 1
							    // 128= éteindre le variateur de la grande chambre
	    Scenario_Eteindre_PDE_LSB,	// 0= ne rien faire / combinaison des valeurs suivantes pour éteindre les lampes voulues
							    // 1  = éteindre la lampe de la cuisine
							    // 2  = éteindre la lampe de la cuisine
							    // 4  = éteindre la lampe de la salle de bain 1
							    // 8  = éteindre la lampe de la salle de bain 2
							    // 16  = éteindre la lampe de la salle de bain 2
							    // 32 = éteindre la lampe du WC 1
							    // 64 = éteindre la lampe du WC 2
							    // 128= éteindre la lampe de service
	    Scenario_Eteindre_PDE_MSB,	// 0= ne rien faire / combinaison des valeurs suivantes pour éteindre les lampes voulues
							    // 1  = éteindre la lampe du dégagement
							    // 2  = éteindre la lampe du dégagement
							    // 4  = éteindre la lampe de la terrasse
							    // 8  = éteindre la lampe de l'annexe
							    // 16 = éteindre la lampe de l'annexe
							    // 128= éteindre le variateur de la salle de bain 1
	    Scenario_Allumer_PDV_LSB,	// 0  = ne rien faire / combinaison des valeurs suivantes pour allumer les lampes voulues
							    // 1  = allumer la lampe de l'entrée
							    // 2  = allumer la lampe du salon
							    // 4  = allumer la lampe du salon
							    // 8  = allumer la lampe du dressing
							    // 16 = allumer la lampe du dressing
	    Scenario_Allumer_PDV_MSB,	// 0  = ne rien faire / combinaison des valeurs suivantes pour allumer les lampes voulues 
							    // 32 = allumer le variateur du bureau
							    // 64 = allumer le variateur de la salle à manger
							    // 128= allumer le variateur du salon
	    Scenario_Allumer_CHB_LSB,	// 0  = ne rien faire / combinaison des valeurs suivantes pour allumer les lampes voulues
							    // 1  = allumer la lampe de l'escalier
							    // 2  = allumer la lampe de la grande chambre
							    // 4  = allumer la lampe de la grande chambre
							    // 8  = allumer la lampe de la petite chambre 1
							    // 16 = allumer la lampe de la petite chambre 1
							    // 32 = allumer la lampe de la petite chambre 2
							    // 64 = allumer la lampe de la petite chambre 3
	    Scenario_Allumer_CHB_MSB,	// 0  = ne rien faire / combinaison des valeurs suivantes pour allumer les lampes voulues
							    // 16 = allumer le variateur de la petite chambre 3
							    // 32 = allumer le variateur de la petite chambre 2
							    // 64 = allumer le variateur de la petite chambre 1
							    // 128= allumer le variateur de la grande chambre
	    Scenario_Allumer_PDE_LSB,	// 0  = ne rien faire / combinaison des valeurs suivantes pour allumer les lampes voulues
							    // 1  = allumer la lampe de la cuisine
							    // 2  = allumer la lampe de la cuisine
							    // 4  = allumer la lampe de la salle de bain 1
							    // 8  = allumer la lampe de la salle de bain 2
							    // 16  = allumer la lampe de la salle de bain 2
							    // 32 = allumer la lampe du WC 1
							    // 64 = allumer la lampe du WC 2
							    // 128= allumer la lampe de service
	    Scenario_Allumer_PDE_MSB,	// 0  = ne rien faire / combinaison des valeurs suivantes pour allumer les lampes voulues
							    // 1  = allumer la lampe du dégagement
							    // 2  = allumer la lampe du dégagement
							    // 4  = allumer la lampe de la terrasse
							    // 8  = allumer la lampe de l'annexe
							    // 16 = allumer la lampe de l'annexe
							    // 128= allumer le variateur de la salle de bain 1
	    // volets & store					
	    Scenario_OuvrirVolets_PDV,	// 0  = ne rien faire / combinaison des valeurs suivantes pour ouvrir les volets voulus
							    // 1  = ouvrir le volet du salon
							    // 2  = ouvrir le volet du salon
							    // 4  = ouvrir le volet du salon
							    // 8  = ouvrir le volet de la salle à manger
							    // 16 = ouvrir le volet de la salle à manger
							    // 32 = ouvrir le volet du bureau
	    Scenario_OuvrirVolets_CHB,	// 0  = ne rien faire / combinaison des valeurs suivantes pour ouvrir les volets voulus
							    // 1  = ouvrir le volet de la grande chambre
							    // 2  = ouvrir le volet de la grande chambre
							    // 4  = ouvrir le volet de la petite chambre 1
							    // 8  = ouvrir le volet de la petite chambre 2
							    // 16 = ouvrir le volet de la petite chambre 3
	    Scenario_OuvrirVolets_PDE,	// 0  = ne rien faire / combinaison des valeurs suivantes pour ouvrir les volets voulus
							    // 1  = ouvrir le volet de la cuisine
							    // 2  = ouvrir le volet de la cuisine
							    // 4  = ouvrir le volet de la salle de bain 1
							    // 8  = ouvrir le store de la terrasse
	    Scenario_FermerVolets_PDV,	// 0  = ne rien faire / combinaison des valeurs suivantes pour fermer les volets voulus
							    // 1  = fermer le volet du salon
							    // 2  = fermer le volet du salon
							    // 4  = fermer le volet du salon
							    // 8  = fermer le volet de la salle à manger
							    // 16 = fermer le volet de la salle à manger
							    // 32 = fermer le volet du bureau
	    Scenario_FermerVolets_CHB,	// 0  = ne rien faire / combinaison des valeurs suivantes pour fermer les volets voulus
							    // 1  = fermer le volet de la grande chambre
							    // 2  = fermer le volet de la grande chambre
							    // 4  = fermer le volet de la petite chambre 1
							    // 8  = fermer le volet de la petite chambre 2
							    // 16 = fermer le volet de la petite chambre 3
	    Scenario_FermerVolets_PDE,	// 0  = ne rien faire / combinaison des valeurs suivantes pour fermer les volets voulus
							    // 1  = fermer le volet de la cuisine
							    // 2  = fermer le volet de la cuisine
							    // 4  = fermer le volet de la salle de bain 1
							    // 8  = replier le store de la terrasse
	    // sécurité
	    Scenario_Securite,					// 0= ne rien faire, 1= couper les prises de sécurité, 2= remettre les prises de sécurité
	    Scenario_Machines,					// 0= ne rien faire, 1= couper les machines à laver, 2= remettre les machines à laver
	
	    // chauffage : consigne à appliquer immédiatement
	    // 0= ne rien faire, 1= mode automatique / 2= mode anticipé / 10 à 15 : consigne de forçage
	    // 10= OFF / 11=CONFORT / 12= ECO / 13= ECO+ / 14= ECO++ / 15= HORS GEL
	    Scenario_Chauf_zj,					// consigne de chauffage zone jour
	    Scenario_Chauf_zn,					// consigne de chauffage zone nuit
	    Scenario_Chauf_zsb1,				// consigne de chauffage zone salle de bain 1
	    Scenario_Chauf_zsb2,				// consigne de chauffage zone salle de bain 2
	
	    // cumulus
	    Scenario_Cumulus,					// 0= ne rien faire, 1= Autonome (ON) / 2= gestion heures creuses / 3= OFF
	    // réveil
	    Scenario_Reveil_Reglage,			// 1 = dérouler la procédure de réglage des réveils
	    Scenario_Reveil_ON,					// 1 = armer la fonction réveil
	    // scénario
	    Scenario_Efface,					// 1= scénario à effacer (remise à 0 de tous les paramètres du scénario)
										    // 2 à 6 = init par défaut de scénario prédéfini
										    // 0= ne rien faire / commande prise en compte par le BP
	    Scenario_NB_VALEURS
    };


    public enum Tbb_Donnees_Index
    {
    // ----------------					version					----------------
	    Version_BP,										// Version carte (revA=1, revB=2, ...)
	    Version_SoftBP,									// Version soft BP (version1=1, version2=2, ...)
	    Version_IHM,									// version carte IHM (revA=1, revB=2, ...)
	    Version_SoftIHM,								// Version soft BP (version1=1, version2=2, ...)
	    Version_TableEchange,							// version table de données (pour vérification compatibilité)
	
    // ----------------		heure / date du boitier principal	----------------
	    Minutes,					// ! ne pas modifié l'ordre de déclaration sans reprendre le code associé !
	    Heure,						// ! Minutes est utilisé comme indice de base !
	    Jour,						// Mise a jour de ces champs en une fois sous MUTEX xxx
	    Mois,
	    Annee,
			
    // ----------------				Etat du système				----------------
	    Status,			
		    // bit 0 : 1= heures creuses en cours
		    // bit 1 : 1= délestage en cours
		    // bit 2 : 1= mode secouru
	    Alerte,	
		    // bit 0 : 1= déclenchement alarme
		    // bit 1 : 1= déclenchement alerte suite à détection fuite d'eau lave linge
		    // bit 2 : 1= déclenchement alerte suite à détection fuite d'eau lave vaisselle
	    Information,	
		    // bit 0 : 1= défaut com compteur ERDF
		    // bit 1 : 1= défaut com IHM
		    // bit 2 : 1= défaut com BA PDV 
		    // bit 3 : 1= défaut com BA CHB
		    // bit 4 : 1= défaut com BA PDE
	
    // ----------------				chauffage					----------------
	    // mode automatique : programmation horaire sur 1 semaine
	    // 1 ordre pour 1 heure sur 7 jours : 6 modes possibles (4 bits) (cf consigne mode de chauffage)
	    // soit 84 octets 
	    Chauf_zj_Auto,														// chauffage zone jour
	    Chauf_zn_Auto	= Chauf_zj_Auto + Constantes.uc_PLANNING_CHAUFFAGE_TAILLE,		// chauffage zone nuit
	    Chauf_zsb1_Auto	= Chauf_zn_Auto + Constantes.uc_PLANNING_CHAUFFAGE_TAILLE,	 	// chauffage salle de bain 1
	    Chauf_zsb2_Auto	= Chauf_zsb1_Auto + Constantes.uc_PLANNING_CHAUFFAGE_TAILLE,	// chauffage salle de bain 2
	    // mode de chauffage
	    // b0-b3 : consigne : 0= OFF / 1=CONFORT / 2= ECO / 3= ECO+ / 4= ECO++ / 5= HORS GEL
	    // b4-b5 : mode 	: 0= automatique / 1= forcé / 2= anticipé
	    // =>  0 à 5 	= mode automatique et consigne en cours
	    // => 16 à 21	= mode forcé et consigne de forçage
	    // => 32 à 37	= mode anticipé et consigne en cours
	    Chauf_zj_Mode = Chauf_zsb2_Auto + Constantes.uc_PLANNING_CHAUFFAGE_TAILLE,		// chauffage zone jour - PROTECTION PAR MUTEX
	    Chauf_zn_Mode,														// chauffage zone nuit - PROTECTION PAR MUTEX
	    Chauf_zsb1_Mode,													// chauffage salle de bain 1 - PROTECTION PAR MUTEX
	    Chauf_zsb2_Mode,													// chauffage salle de bain 2 - PROTECTION PAR MUTEX
	    // MUTEX : un nouvel ordre peut provenir de l'écran pendant que BP est en train de traiter cet octet
	    //         sans mutex, l'ordre risque d'être effacé
	    //		   avec mutex, l'ordre de l'écran sera pris en compte après traitement donc pas de risque de perte...
	
    // ----------------				Cumulus						----------------
	    Cumulus_Mode,				// 0 = Autonome (ON) / 1 = gestion heures creuses / 2 = OFF

    // ----------------					vacances				----------------
	    // date et heure de fin de vacances : 
	    VacanceFin_H,								// heure //xxx a proteger par mutex // xxx traitement a ecrire
	    VacanceFin_Mn,								// minute 
	    VacanceFin_J,								// jour 
	    VacanceFin_M,								// mois 
	    VacanceFin_A,								// année 
	    // chauffage
	    // fin de vacances : consigne forcée
	    // 0 = pas de forçage / 10 à 15 : consigne de forçage
	    // 10 = CONFORT / 11 = ECO  / 12 = ECO+ / 13 = ECO++ / 14 = HORS GEL / 15 = ARRET
	    VacanceFin_zj_Force,						// chauffage zone jour
	    VacanceFin_zn_Force,						// chauffage zone nuit
	    VacanceFin_zsb1_Force,						// chauffage salle de bain 1
	    VacanceFin_zsb2_Force,						// chauffage salle de bain 2
	
    // ----------------					arrosage				----------------
	    // sélection du mode
	    Arrose_Mode,			// 0 = OFF : pas d'arrosage
							    // 1 à 254 : durée d'arrosage en mode forcé
							    // 255 : mode automatique : pilotage en fonction du planning horaire
	    // mode automatique : programmation horaire sur 1 semaine
	    Arrose_Auto,			// 1 ordre pour 30 minutes sur 7 jours : 2 modes possibles (1 bit), soit 42 octets
	    // détecteur de pluie
        Arrose_Detect = Arrose_Auto + Constantes.uc_PLANNING_ARROSAGE_TAILLE,	// 0 = détecteur actif / > 0 = détecteur dévalidé			
	
    // ----------------					alarme					----------------
	    Alarme_AccesADistance,		// Autorise (1) ou non (0) la modification de l'état de l'alarme à distance = 407
	    Alarme_Mode,				// Définie le mode de fonctionnement de l'alarme
								    // 		0x00	Le système d'alarme n'est pas utilisé
								    //  	0x01	Mode réglage
								    //  	0x02	Alarme indépendante
								    //		0x03	Alarme sur scénario "Je sors"
								    //		0x04	Alarme sur scénario "Je vais me coucher"
								    //		0x05	Alarme sur scénario "Je pars en vacances"
								    //		0x06	Alarme sur scénario "Personnalisé"
	    Alarme_Commande,			// 1 Demande de mise sous alarme -> repassé à 0 par BP après prise en compte
	    Alarme_CodeSaisiLSB,		// Bit 0-3 : 1er chiffre (le + à gauche à l'écran) - Bit 4-7 : 2eme chiffre
	    Alarme_CodeSaisiMSB,		// Bit 0-3 : 3eme chiffre - Bit 4-7 : 4eme chiffre (le + à droite à l'écran)
								    // Mettre / désactiver alarme si code LSB / MSB correct -> remis à 0xFFFF par BP après prise en compte
	    Alarme_Autorisation,		// 0x00 : traitement en attente
								    // 0x01 : code valide
								    // 0x02 : code invalide
	    Alarme_SuiviAlarme,			// Suit le déroulement des étapes de l'automate
								    // 		0x00	Etape de départ
								    //  	0x01	Mise sous alarme impossible -> problème d'alimentation
								    //  	0x02	Mise sous alarme impossible -> intrusion ou vandalisme
								    //		0x03	Procédure de sortie
								    //		0x04	Régime de croisière
								    //		0x05	Procédure d'entrée
								    //		0x06	Intrusion ou vandalisme
	    Alarme_Detection,			// Par bit - 0 : pas de détection - 1 : détection
								    //		bit 0 : état du détecteur d'ouverture
								    //		bit 1 : état du détecteur présence 1
								    //		bit 2 : état du détecteur présence 2
	    Alarme_Fraude,				// Par bit - 0 : pas de fraude - 1 : fraude
								    //		bit 0 : état de fraude tableau domotique
								    //		bit 1 : état de fraude IHM
								    //		bit 2 : état de fraude détecteur présence 1
								    //		bit 3 : état de fraude détecteur présence 2
								    //		bit 4 : état de fraude sirène d'intérieur
								    //		bit 5 : état de fraude sirène d'extérieur
	    Alarme_SuiviChangementCode,	// 0x00 : état de départ
								    // 0x01 : 1er envoi nouveau code
								    // 0x02 : 2ème envoi nouveau code
	    Alarme_CodeUser1LSB,		// Code alarme 1 Bit 0-3 : 1er chiffre (le + à gauche) - Bit 4-7 : 2eme chiffre
	    Alarme_CodeUser1MSB,		// Code alarme 1 Bit 0-3 : 3eme chiffre - Bit 4-7 : 4eme chiffre (le + à droite)
	    Alarme_CodeUser2LSB,		// Code alarme 2 Bit 0-3 : 1er chiffre (le + à gauche) - Bit 4-7 : 2eme chiffre						- NON UTILISE ACTUELLEMENT !
	    Alarme_CodeUser2MSB,		// Code alarme 2 Bit 0-3 : 3eme chiffre - Bit 4-7 : 4eme chiffre (le + à droite)					- NON UTILISE ACTUELLEMENT !
	    Alarme_CodeSuperUserLSB,	// Code alarme super utilisateur Bit 0-3 : 1er chiffre (le + à gauche) - Bit 4-7 : 2eme chiffre		- NON UTILISE ACTUELLEMENT !
	    Alarme_CodeSuperUserMSB,	// Code alarme super utilisateur Bit 0-3 : 3eme chiffre - Bit 4-7 : 4eme chiffre (le + à droite)	- NON UTILISE ACTUELLEMENT !
	    Alarme_TestRAZPresence,		// Mode réglage -> remis à 0 par BP après prise en compte demande
								    //		0x00 : ne fait rien
								    //		0x01 : remet le bit du détecteur de présence 1 du registre Alarme_Detection à "pas de détection"
								    //		0x02 : remet le bit du détecteur de présence 2 du registre Alarme_Detection à "pas de détection"
	    Alarme_TestSirenes,			// Mode réglage
								    //		0x00 : Aucune sirène activée
								    //		0x01 : Sirène d'intérieur activée en alarme
								    //		0x02 : Sirène d'extérieur activée en alarme

	    AlarmeConfig,				// -> voir enumAlarmeConfig
	
    // ----------------			alerte et sécurité					----------------
        Alerte_Intensite = AlarmeConfig + enumAlarmeConfig.AlarmeConfig_NB_VALEURS,	// reglage intensite de la sirène sur alerte : 25= fort / 50= moyen / 75= faible
	    Alerte_Duree,					// reglage durée de l'alerte : 0= jusqu'à disparition / 1 à 255= temps en secondes
	    Alerte_TestSirene,				// 1 = sirène à activer Intensité alerte (activé tant que cette info est non nulle)
	    Alerte_Acquit,					// 1 = acquittement alerte xxx
	    Securite_PriseCoupe,			// 1= prises de sécurité coupées, 0 = pilotées
	    Securite_FuiteLinge,			// 1= détection de fuite sur le lave-linge activée
	    Securite_FuiteVaisselle,		// 1= détection de fuite sur le lave-vaisselle activée
	    Securite_FuiteAlerte,			// 1= alerte sonore activée en cas de fuite d'eau
	
	    // ----------------				réveil					----------------
	    // fonction réveil : provoque l'ouverture des volants roulants
	    // grande chambre
	    Reveil_ChambreGr_H,		// heure
	    Reveil_ChambreGr_Mn,	// minutes
	    Reveil_ChambreGr_ON,	// 0 = fonction désactivée / > 0 = fonction active
	    // chambre 1
	    Reveil_Chambre1_H,		// heure 
	    Reveil_Chambre1_Mn,		// minutes
	    Reveil_Chambre1_ON,		// 0 = fonction désactivée / > 0 = fonction active
	    // chambre 2
	    Reveil_Chambre2_H,		// heure 
	    Reveil_Chambre2_Mn,		// minutes 
	    Reveil_Chambre2_ON,		// 0 = fonction désactivée / 1 = fonction active
	    // chambre 3
	    Reveil_Chambre3_H,		// heure 
	    Reveil_Chambre3_Mn,		// minutes 
	    Reveil_Chambre3_ON,		// 0 = fonction désactivée / 1 = fonction active
	    // bureau
	    Reveil_Bureau_H,		// heure 
	    Reveil_Bureau_Mn,		// minutes
	    Reveil_Bureau_ON,		// 0 = fonction désactivée / 1 = fonction active
		
    // ----------------				délestage					----------------
	    Delestage,						// 0 = fonction désactivée / <>0 : fonction active

    // ----------------		téléinformation du compteur ERDF	----------------
	    TeleInf_OPTARIF,			// option tarifaire du compteur	
	    TeleInf_PTEC,				// période tarifaire en cours	
	    TeleInf_ADPS,				// avertissement dépassement puissance souscrite	
	    TeleInf_PAPP_LSB,			// puissance apparente utilisée	(sur 2 octets)
	    TeleInf_PAPP_MSB,
	    // puissances consommées heures de base ou heures pleines
	    TeleInf_HPB_Global_LSB,			// puissance globale
	    TeleInf_HPB_Global_MSB,	
	    TeleInf_HPB_Chauffage_LSB,		// chauffage
	    TeleInf_HPB_Chauffage_MSB,
	    TeleInf_HPB_Refroid_LSB,		// refroidissement
	    TeleInf_HPB_Refroid_MSB,
	    TeleInf_HPB_EauChaude_LSB,		// Eau chaude
	    TeleInf_HPB_EauChaude_MSB,
	    TeleInf_HPB_Prises_LSB,			// prises
	    TeleInf_HPB_Prises_MSB,	
	    TeleInf_HPB_Autres_LSB,			// autres
	    TeleInf_HPB_Autres_MSB,	
	    // puissances consommées heures creuses
	    TeleInf_HC_Global_LSB,			// puissance globale
	    TeleInf_HC_Global_MSB,	
	    TeleInf_HC_Chauffage_LSB,		// chauffage
	    TeleInf_HC_Chauffage_MSB,
	    TeleInf_HC_Refroid_LSB,			// refroidissement
	    TeleInf_HC_Refroid_MSB,	
	    TeleInf_HC_EauChaude_LSB,		// Eau chaude
	    TeleInf_HC_EauChaude_MSB,
	    TeleInf_HC_Prises_LSB,			// prises
	    TeleInf_HC_Prises_MSB,	
	    TeleInf_HC_Autres_LSB,			// autres
	    TeleInf_HC_Autres_MSB,	
	    // puissances consommées : répartition en % (paramètrage)
	    TeleInf_Repartition_Chauffage,	// chauffage
	    TeleInf_Repartition_Refroid,	// refroidissement
	    TeleInf_Repartition_EauChaude,	// Eau chaude
	    TeleInf_Repartition_Prises,		// prises
	    TeleInf_Repartition_Autres,		// autres

    // ----------------				éclairage 					----------------
    // configuration des variateurs : 0= TOR (avec rampe) / 1= gradateur / 2= TOR (sans rampe)
    // Zone Variateurs_PDV_Conf -> Volets_PDE_Temps : infos BA envoyées par tache et traitée en "un bloc" -> doit rester compact - ne pas insérer de valeurs dans ce bloc
	    Variateurs_PDV_Conf,								// 8 variateurs possibles par boitier
										    //    salon
										    // +1 salle à manger
										    // +2 bureau
										    // +3 à + 7 non utilisé
        Variateurs_CHB_Conf = Variateurs_PDV_Conf + Constantes.uc_NB_VARIATEURS_POSSIBLES_PAR_BA,	
										    //    grande chambre
										    // +1 petite chambre 1
										    // +2 petite chambre 2
										    // +3 petite chambre 3
										    // +4 à +7 non utilisé
        Variateurs_PDE_Conf = Variateurs_CHB_Conf + Constantes.uc_NB_VARIATEURS_POSSIBLES_PAR_BA,		
										    //    salle de bain 1
										    // +1 à +7 non utilisé
    // temps d'extinction (lampes simples) : 1 à 255 minutes / 0= pas d'extinction automatique
	    Lampes_PDV_Temps = Variateurs_PDE_Conf+ Constantes.uc_NB_VARIATEURS_POSSIBLES_PAR_BA,	// 16 sorties simples possibles par boitier
										    // non utilisé
        Lampes_CHB_Temps = Lampes_PDV_Temps + Constantes.uc_NB_LAMPES_POSSIBLES_PAR_BA,
										    // non utilisé
        Lampes_PDE_Temps = Lampes_CHB_Temps + Constantes.uc_NB_LAMPES_POSSIBLES_PAR_BA,
										    // +0 à + 4 non utilisé
										    // +5 WC 1
										    // +6 WC 2
										    // +7 service 
										    // +8 à + 15 non utilisé
	
	    // ----------------			volets / store 				----------------
    // temps d'action (volets, store) : 1 à 255 secondes (0 à 4 minutes)
        Volets_PDV_Temps = Lampes_PDE_Temps + Constantes.uc_NB_LAMPES_POSSIBLES_PAR_BA,		// 8 volets ou stores possibles par boitier
										    //    volet salon
										    // +0 volet salon
										    // +1 volet salon
										    // +2 volet salon
										    // +3 volet salle à manger
										    // +4 volet salle à manger
										    // +5 volet bureau
										    // +6 à + 7 non utilisé
        Volets_CHB_Temps = Volets_PDV_Temps + Constantes.uc_NB_VOLETS_POSSIBLES_PAR_BA,
										    //    volet grande chambre
										    // +1 volet grande chambre
										    // +2 volet petite chambre 1
										    // +3 volet petite chambre 2
										    // +4 volet petite chambre 3
										    // +5 à +7 non utilisé
        Volets_PDE_Temps = Volets_CHB_Temps + Constantes.uc_NB_VOLETS_POSSIBLES_PAR_BA,
										    //    volet cuisine
										    // +1 volet cuisine
										    // +2 volet salle de bain 1
										    // +3 store terrasse
										    // +4 à +7 non utilisé

    // ----------------				scénarios					----------------
        Scenario = Volets_PDE_Temps + Constantes.uc_NB_VOLETS_POSSIBLES_PAR_BA,	// numéro du scénario à lancer
																	    // 0 = aucun / commande prise en compte par le BP
	    // Scenario1 = réservé au serveur Internet (ex: descendre les volets roulants / remonter le store)
	    // Scenario2 = "Je sors" 
	    // Scenario3 = "Je pars en vacances" 
	    // Scenario4 = "Je rentre"
	    // Scenario5 = "Je vais me coucher" 
	    // Scenario6 = "Je me lève"
	    // Scenario7 = "Personnalisé" 
	    // Scenario8 = libre
	
	    Scenario_DernierLance, 							// Dernier scénario lancé

	    Scenario1,										// -> voir enumScenario
        Scenario2 = Scenario1 + enumScenario.Scenario_NB_VALEURS,	// -> voir enumScenario
        Scenario3 = Scenario2 + enumScenario.Scenario_NB_VALEURS,	// -> voir enumScenario
        Scenario4 = Scenario3 + enumScenario.Scenario_NB_VALEURS,	// -> voir enumScenario
        Scenario5 = Scenario4 + enumScenario.Scenario_NB_VALEURS,	// -> voir enumScenario
        Scenario6 = Scenario5 + enumScenario.Scenario_NB_VALEURS,	// -> voir enumScenario
        Scenario7 = Scenario6 + enumScenario.Scenario_NB_VALEURS,	// -> voir enumScenario
        Scenario8 = Scenario7 + enumScenario.Scenario_NB_VALEURS,	// -> voir enumScenario
	
	    // Contiennent l'état du boitier principal (pour le serveur)
	    EtatBP1 = Scenario8 + enumScenario.Scenario_NB_VALEURS,
		    // bit 0 : alarme activée
		    // bit 1 : alarme déclenchée
		    // Autres bits : à 0 (pour évolutions futures)
	    EtatBP2,
		    // A 0 (pour évolutions futures)

        Nb_Tbb_Donnees	
    };

}
