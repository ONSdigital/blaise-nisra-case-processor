Feature: Import Nisra cases
	In order to process cases gathered online
	As a service
	I want to be given cases to import representing the data captured online


@smoke
#Covers all scenarios in one test run
Scenario: There is an online file available that contains cases that already exists in the blaise database, the cases are updated depending on the outcome codes
	Given there is a Nisra file that contains the following cases
	| primarykey | outcome | mode | 
	#scenario 1
	| 900001     | 110     | Web  | 
	#scenario 2
	| 900002     | 210     | Web  | 
	#scenario 3
	| 900003     | 110     | Web  | 
	#scenario 4
	| 900004     | 110     | Web  | 
	| 900005     | 110     | Web  | 
	| 900006     | 110     | Web  | 
	| 900007     | 110     | Web  | 
	| 900008     | 110     | Web  | 
	| 900009     | 110     | Web  | 
	#scenario 5
	| 900010     | 0       | Web  | 
	#scenario 6
	| 900011     | 210     | Web  | 
	#scenario 7
	| 900012     | 210     | Web  | 
	| 900013     | 210     | Web  | 
	| 900014     | 210     | Web  | 
	| 900015     | 210     | Web  | 
	| 900016     | 210     | Web  | 
	| 900017     | 210     | Web  | 
	#scenario 8
	| 900018     | 0       | Web  | 
	#scenario 9
	| 900019     | 210     | Web  | 
	#scenario 10
	| 900020     | 110     | Web  | 
	#defect where we had a code of 580 in tel and were not overwriting it with the web case
	| 900021     | 110     | Web  | 
	
	And blaise contains the following cases
	| primarykey | outcome | mode | 
	#scenario 1
	| 900001     | 110     | Tel  | 
	#scenario 2
	| 900002     | 110     | Tel  | 
	#scenario 3
	| 900003     | 210     | Tel  | 
	#scenario 4
	| 900004     | 310     | Tel  | 
	| 900005     | 430     | Tel  | 
	| 900006     | 460     | Tel  | 
	| 900007     | 461     | Tel  | 
	| 900008     | 541     | Tel  | 
	| 900009     | 542     | Tel  | 
	#scenario 5
	| 900010     | 110     | Tel  | 
	#scenario 6
	| 900011     | 210     | Tel  | 
	#scenario 7
	| 900012     | 310     | Tel  | 
	| 900013     | 430     | Tel  | 
	| 900014     | 460     | Tel  | 
	| 900015     | 461     | Tel  | 
	| 900016     | 541     | Tel  | 
	| 900017     | 542     | Tel  | 
	#scenario 8
	| 900018     | 310     | Tel  | 
	#scenario 9
	| 900019     | 562     | Tel  | 
	#scenario 10
	| 900020     | 561     | Tel  | 
	#defect where we had a code of 580 in tel and were not overwriting it with the web case
	| 900021     | 580     | Tel  | 

	When the nisra file is processed
	Then blaise will contain the following cases
	| primarykey | outcome | mode | 
	#scenario 1
	| 900001     | 110     | Web  | 
	#scenario 2
	| 900002     | 110     | Tel  | 
	#scenario 3
	| 900003     | 110     | Web  | 
	#scenario 4
	| 900004     | 110     | Web  | 
	| 900005     | 110     | Web  | 
	| 900006     | 110     | Web  | 
	| 900007     | 110     | Web  | 
	| 900008     | 110     | Web  | 
	| 900009     | 110     | Web  | 
	#scenario 5
	| 900010     | 110     | Tel  | 
	#scenario 6
	| 900011     | 210     | Web  | 
	#scenario 7
	| 900012     | 210     | Web  | 
	| 900013     | 210     | Web  | 
	| 900014     | 210     | Web  | 
	| 900015     | 210     | Web  | 
	| 900016     | 210     | Web  | 
	| 900017     | 210     | Web  | 
	#scenario 8
	| 900018     | 310     | Tel  | 
	#scenario 9
	| 900019     | 562     | Tel  | 
	#scenario 10
	| 900020     | 561     | Tel  | 
	#defect where we had a code of 580 in tel and were not overwriting it with the web case
	| 900021     | 110     | Web  | 

#Scenario 1 https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow
Scenario: A case in the NISRA file is complete and in Blaise it is complete, we take the NISRA case
	Given there is a Nisra file that contains a case that is complete
	And the same case exists in Blaise that is complete
	When the nisra file is processed
	Then the existing blaise case is overwritten with the NISRA case

#Scenario 2 https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow
Scenario:  A case in the NISRA file is partially complete and in Blaise it is complete, we keep the existing blaise case
	Given there is a Nisra file that contains a case that is partially complete
	And the same case exists in Blaise that is complete
	When the nisra file is processed
	Then the existing blaise case is kept

#Scenario 3 https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow
Scenario: A case in the NISRA file is complete and in Blaise it is partially complete, we take the NISRA case
	Given there is a Nisra file that contains a case that is complete
	And the same case exists in Blaise that is partially complete
	When the nisra file is processed
	Then the existing blaise case is overwritten with the NISRA case

#Scenario 4 https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow
Scenario Outline: A case in the NISRA file is complete and in Blaise it is between the range 210-542, we take the NISRA case
	Given there is a Nisra file that contains a case that is complete
	And the same case exists in Blaise with the outcome code '<existingOutcome>'
	When the nisra file is processed
	Then the existing blaise case is overwritten with the NISRA case
	Examples: 
	| existingOutcome | description                                                 |
	| 210             | Partially completed survey                                  |
	| 310             | Non-contact                                                 |
	| 430             | HQ refusal                                                  |
	| 440             | Person not available                                        |
	| 460             | Refuses cooperation - hard refusal                          |
	| 461             | Refuses cooperation - soft refusal could be contacted again |
	| 541             | Language difficulties - notified by Head Office             |
	| 542             | Language difficulties - notified to interviewer             |

#Scenario 5 https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow
Scenario: A case in the NISRA file that has not started and in Blaise it is complete, we keep the existing blaise case
	Given there is a Nisra file that contains a case that has not been started
	And the same case exists in Blaise that is complete
	When the nisra file is processed
	Then  the existing blaise case is kept

#Scenario 6 https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow
Scenario: A case in the NISRA file is partially complete and in Blaise it is partially complete, we take the NISRA case
	Given there is a Nisra file that contains a case that is partially complete
	And the same case exists in Blaise that is partially complete
	When the nisra file is processed
	Then the existing blaise case is overwritten with the NISRA case

#Scenario 7 https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow
Scenario Outline: A case in the NISRA file is partially complete and in Blaise and it is between the range 310-542, we take the NISRA case
	Given there is a Nisra file that contains a case that is complete
	And the same case exists in Blaise with the outcome code '<existingOutcome>'
	When the nisra file is processed
	Then the existing blaise case is overwritten with the NISRA case
	Examples: 
	| existingOutcome | description                                                 |
	| 310             | Non-contact                                                 |
	| 430             | HQ refusal                                                  |
	| 440             | Person not available                                        |
	| 460             | Refuses cooperation - hard refusal                          |
	| 461             | Refuses cooperation - soft refusal could be contacted again |
	| 541             | Language difficulties - notified by Head Office             |
	| 542             | Language difficulties - notified to interviewer             |

#Scenario 8 https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow
Scenario: A case in the NISRA file that has not started and in Blaise it is non-contact, we keep the existing blaise case
	Given there is a Nisra file that contains a case that has not been started
	And the same case exists in Blaise with the outcome code '310'
	When the nisra file is processed
	Then  the existing blaise case is kept

#Scenario 9 https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow
Scenario: A case in the NISRA file that is partially complete and in Blaise it marked as respondent request for data to be deleted, we keep the existing blaise case
	Given there is a Nisra file that contains a case that is partially complete
	And the same case exists in Blaise with the outcome code '562'
	When the nisra file is processed
	Then  the existing blaise case is kept

#Scenario 10 https://collaborate2.ons.gov.uk/confluence/display/QSS/Blaise+5+NISRA+Case+Processor+Flow
Scenario: A case in the NISRA file that is complete and in Blaise it marked as respondent request for data to be deleted, we keep the existing blaise case
	Given there is a Nisra file that contains a case that is complete
	And the same case exists in Blaise with the outcome code '561'
	When the nisra file is processed
	Then  the existing blaise case is kept

#Scenario 11 https://collaborate2.ons.gov.uk/confluence/display/QSS/OPN+NISRA+Case+Processing+Scenarios
Scenario: A case in the Nisra file has a better outcome but the case has been updated recently so may be open in Cati, do not update
	Given there is a Nisra file that contains a case that is complete
	And the same case exists in Blaise that is partially complete
	And the case has been updated within the past 30 minutes
	When the nisra file is processed
	Then the existing blaise case is kept

#Scenario 12 https://collaborate2.ons.gov.uk/confluence/display/QSS/OPN+NISRA+Case+Processing+Scenarios
Scenario: A case in the nisra file has already been processed
	Given there is a Nisra file that contains a case that is complete
	When the nisra file is processed
	Then the nisra case is not imported again