Feature: Load test the Nisra import process
	In order to process cases gathered online
	As a service
	I want to be given cases to import representing the data captured online

Scenario: Load tests x iterations every y minutes for z hours
	Given there is a Nisra file that contains '5000' cases 
	And blaise contains no cases
	When the nisra file is triggered every '15' minutes for '1' hour(s)
	Then blaise will contain '5000' cases