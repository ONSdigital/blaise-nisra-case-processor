Feature: Import Nisra cases
	In order to process cases gathered online
	As a service
	I want to be given cases to import representing the data captured online

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

#Additional NFR Scenarios 
Scenario: There is a no Nisra file available and Blaise contains no cases
	Given there is a not a Nisra file available 
	And blaise contains no cases
	When the nisra file is processed
	Then blaise will contain no cases

Scenario: There is a no Nisra file available and Blaise contains 10 cases
	Given there is a not a Nisra file available 
	And blaise contains '10' cases
	When the nisra file is processed
	Then blaise will contain '10' cases

Scenario: There is a Nisra file available with 10 cases and Blaise contains no cases
	Given there is a Nisra file that contains '10' cases 
	And blaise contains no cases
	When the nisra file is processed
	Then blaise will contain '10' cases

#LU-7618 Take existing Case ID when importing Nisra records
Scenario: A case in the NISRA file is complete and has no case Id and in Blaise it is complete with a case Id, we take the NISRA case and keep the existing case Id
	Given there is a Nisra file that contains a case that is complete
	And the same case exists in Blaise that is complete and has a case id of '30001'
	When the nisra file is processed
	Then the existing blaise case is overwritten with the NISRA case
	And the case has a case id of '30001' 

#Covers all scenarios in one test run
Scenario: There is a Nisra file available that contains cases that already exists in the blaise database, the cases are updated depending on the outcome codes
	Given there is a Nisra file that contains the following cases
	| primarykey | outcome | mode | caseid |
	#scenario 1
	| 900001     | 110     | Web  | 0      |
	#scenario 2
	| 900002     | 210     | Web  | 0      |
	#scenario 3
	| 900003     | 110     | Web  | 0      |
	#scenario 4
	| 900004     | 110     | Web  | 0      |
	| 900005     | 110     | Web  | 0      |
	| 900006     | 110     | Web  | 0      |
	| 900007     | 110     | Web  | 0      |
	| 900008     | 110     | Web  | 0      |
	| 900009     | 110     | Web  | 0      |
	#scenario 5
	| 900010     | 0       | Web  | 0      |
	#scenario 6
	| 900011     | 210     | Web  | 0      |
	#scenario 7
	| 900012     | 210     | Web  | 0      |
	| 900013     | 210     | Web  | 0      |
	| 900014     | 210     | Web  | 0      |
	| 900015     | 210     | Web  | 0      |
	| 900016     | 210     | Web  | 0      |
	| 900017     | 210     | Web  | 0      |
	#scenario 8
	| 900018     | 0       | Web  | 0      |
	#scenario 9
	| 900019     | 210     | Web  | 0      |
	#scenario 10
	| 900020     | 110     | Web  | 0      |
	
	And blaise contains the following cases
	| primarykey | outcome | mode | caseid |
	#scenario 1
	| 900001     | 110     | Tel  | 3001   |
	#scenario 2
	| 900002     | 110     | Tel  | 3002   |
	#scenario 3
	| 900003     | 210     | Tel  | 3003   |
	#scenario 4
	| 900004     | 310     | Tel  | 3004   |
	| 900005     | 430     | Tel  | 3005   |
	| 900006     | 460     | Tel  | 3006   |
	| 900007     | 461     | Tel  | 3007   |
	| 900008     | 541     | Tel  | 3008   |
	| 900009     | 542     | Tel  | 3009   |
	#scenario 5
	| 900010     | 110     | Tel  | 3010   |
	#scenario 6
	| 900011     | 210     | Tel  | 3011   |
	#scenario 7
	| 900012     | 310     | Tel  | 3012   |
	| 900013     | 430     | Tel  | 3013   |
	| 900014     | 460     | Tel  | 3014   |
	| 900015     | 461     | Tel  | 3015   |
	| 900016     | 541     | Tel  | 3016   |
	| 900017     | 542     | Tel  | 3017   |
	#scenario 8
	| 900018     | 310     | Tel  | 3018   |
	#scenario 9
	| 900019     | 562     | Tel  | 3019   |
	#scenario 10
	| 900020     | 561     | Tel  | 3020   |

	When the nisra file is processed
	Then blaise will contain the following cases
	| primarykey | outcome | mode | caseid |
	#scenario 1
	| 900001     | 110     | Web  | 3001   |
	#scenario 2
	| 900002     | 110     | Tel  | 3002   |
	#scenario 3
	| 900003     | 110     | Web  | 3003   |
	#scenario 4
	| 900004     | 110     | Web  | 3004   |
	| 900005     | 110     | Web  | 3005   |
	| 900006     | 110     | Web  | 3006   |
	| 900007     | 110     | Web  | 3007   |
	| 900008     | 110     | Web  | 3008   |
	| 900009     | 110     | Web  | 3009   |
	#scenario 5
	| 900010     | 110     | Tel  | 3010   |
	#scenario 6
	| 900011     | 210     | Web  | 3011   |
	#scenario 7
	| 900012     | 210     | Web  | 3012   |
	| 900013     | 210     | Web  | 3013   |
	| 900014     | 210     | Web  | 3014   |
	| 900015     | 210     | Web  | 3015   |
	| 900016     | 210     | Web  | 3016   |
	| 900017     | 210     | Web  | 3017   |
	#scenario 8
	| 900018     | 310     | Tel  | 3018   |
	#scenario 9
	| 900019     | 562     | Tel  | 3019   |
	#scenario 10
	| 900020     | 561     | Tel  | 3020   |


Scenario: Test Nisra Mover
	Given there is a Nisra file that contains '50' cases 
	And blaise contains no cases
	When the nisra file is processed
	Then blaise will contain '10' cases