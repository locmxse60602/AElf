# Parliament Contract

## **Actions**

### **Initialize**

```Protobuf
rpc Initialize(InitializeInput) returns (google.protobuf.Empty) {}

message InitializeInput{
    aelf.Address privileged_proposer = 1;
    bool proposer_authority_required = 2;
}
```

**Initialize** will set parliament proposer whitelist and create the first parliament organization with specific **proposer_authority_required**.

- **InitializeInput**
  - **privileged proposer**: privileged proposer would be the first address in parliament proposer whitelist.
  - **proposer authority required**: the setting indicates if proposals need authority to be created for first/default parliament organization.

### **CreateOrganization**

```Protobuf
rpc CreateOrganization (CreateOrganizationInput) returns (aelf.Address) { }

message CreateOrganizationInput {
    acs3.ProposalReleaseThreshold proposal_release_threshold = 1;
    bool proposer_authority_required = 2;
    bool parliament_member_proposing_allowed = 3;
}

message OrganizationCreated{
    option (aelf.is_event) = true;
    aelf.Address organization_address = 1;
}
```

Creates parliament organization with input data.

- **CreateOrganizationInput**
  - **ProposalReleaseThreshold**: the threshold for releasing the proposal.
  - **proposer authority required**: setting this to true can allow anyone to create proposals.
  - **parliament member proposing allowed**: setting this to true can allow parliament member to create proposals.

- **ProposalReleaseThreshold**
  - **minimal approval threshold**: the value to be divided by ``10000`` for the minimum approval threshold in fraction.
  - **maximal rejection threshold**: the value to be divided by ``10000`` for the maximal rejection threshold in fraction.
  - **maximal abstention threshold**: the value to be divided by ``10000`` for the maximal abstention threshold in fraction.
  - **minimal vote threshold**: the value to be divided by ``10000`` for the minimal vote threshold in fraction.

- **Returns**
  - **Address**: the address of newly created organization.

- **Events**
  - **OrganizationCreated**
    - **organization address**: the address of newly created organization.

### **CreateOrganizationBySystemContract**

```Protobuf
rpc CreateOrganizationBySystemContract(CreateOrganizationBySystemContractInput) returns (aelf.Address){}

message CreateOrganizationBySystemContractInput {
    CreateOrganizationInput organization_creation_input = 1;
    string organization_address_feedback_method = 2;
}

message OrganizationCreated{
    option (aelf.is_event) = true;
    aelf.Address organization_address = 1;
}
```

Creates parliament organization when called by system contract.

- **CreateOrganizationBySystemContractInput**
  - **CreateOrganizationInput**: the parameters of creating the organization.
  - **organization address feedback method**: organization address callback method which replies the organization address to caller contract.

note: *for CreateOrganizationInput see CreateOrganization*

- **Returns**
  - **Address**: the address of newly created organization.

- **Events**
  - **OrganizationCreated**
    - **organization address**: the address of newly created organization.

## **ACS3 specific methods**

### **CreateProposal**

```Protobuf
rpc CreateProposal (CreateProposalInput) returns (aelf.Hash) { }

message CreateProposalInput {
    string contract_method_name = 1;
    aelf.Address to_address = 2;
    bytes params = 3;
    google.protobuf.Timestamp expired_time = 4;
    aelf.Address organization_address = 5;
    string proposal_description_url = 6,
    aelf.Hash token = 7;
}

message ProposalCreated{
    option (aelf.is_event) = true;
    aelf.Hash proposal_id = 1;
}
```

This method creates a proposal for which organization members can vote. When the proposal is released, a transaction will be sent to the specified contract.

- **CreateProposalInput**
  - **contract method name**: the name of the method to call after release.
  - **to address**: the address of the contract to call after release.
  - **expiration**: the timestamp at which this proposal will expire.
  - **organization address**: the address of the organization.
  - **proposal_description_url**: the url is used for proposal describing.
  - **token**: the token is for proposal id generation and with this token, proposal id can be calculated before proposing. 

- **Returns**
  - **Hash**: id of the newly created proposal.

- **Events**
  - **ProposalCreated**
    - **proposal_id**: id of the created proposal.

### **Approve**

```Protobuf
rpc Approve (aelf.Hash) returns (google.protobuf.Empty){}

message ReceiptCreated {
    option (aelf.is_event) = true;
    aelf.Hash proposal_id = 1;
    aelf.Address address = 2;
    string receipt_type = 3;
    google.protobuf.Timestamp time = 4;
}
```

This method is called to approve the specified proposal.

- **Hash**: id of the proposal.

- **Events**
  - **ReceiptCreated**
    - **proposal id**: id of the proposal.
    - **address**: send address who votes for approval.
    - **receipt type**: Approve.
    - **time**: timestamp of this method call.

### **Reject**

```Protobuf
rpc Reject(aelf.Hash) returns (google.protobuf.Empty){}

message ReceiptCreated {
    option (aelf.is_event) = true;
    aelf.Hash proposal_id = 1;
    aelf.Address address = 2;
    string receipt_type = 3;
    google.protobuf.Timestamp time = 4;
}
```

This method is called to reject the specified proposal.

- **Hash**: id of the proposal.

- **Events**
  - **ReceiptCreated**
    - **proposal id**: id of the proposal.
    - **address**: send address who votes for reject.
    - **receipt type**: Reject.
    - **time**: timestamp of this method call.


### **Abstain**

```Protobuf
rpc Abstain(aelf.Hash) returns (google.protobuf.Empty){}

message ReceiptCreated {
    option (aelf.is_event) = true;
    aelf.Hash proposal_id = 1;
    aelf.Address address = 2;
    string receipt_type = 3;
    google.protobuf.Timestamp time = 4;
}
```

This method is called to abstain from the specified proposal.

- **Hash**: id of the proposal.

- **Events**
  - **ReceiptCreated**
    - **proposal id**: id of the proposal.
    - **address**: send address who votes for abstention.
    - **receipt type**: Abstain.
    - **time**: timestamp of this method call.

### **Release**

```Protobuf
rpc Release(aelf.Hash) returns (google.protobuf.Empty){}
```

This method is called to release the specified proposal.

-**Hash**: id of the proposal.

- **Events**
  - **ProposalReleased**
    - **proposal id**: id of the proposal.

### **ChangeOrganizationThreshold**

```Protobuf
rpc ChangeOrganizationThreshold(ProposalReleaseThreshold) returns (google.protobuf.Empty){}

message ProposalReleaseThreshold {
    int64 minimal_approval_threshold = 1;
    int64 maximal_rejection_threshold = 2;
    int64 maximal_abstention_threshold = 3;
    int64 minimal_vote_threshold = 4;
}

message OrganizationThresholdChanged{
    option (aelf.is_event) = true;
    aelf.Address organization_address = 1;
    ProposalReleaseThreshold proposer_release_threshold = 2;
}
```

This method changes the thresholds associated with proposals. All fields will be overwritten by the input value and this will affect all current proposals of the organization. Note: only the organization can execute this through a proposal.

- **ProposalReleaseThreshold**
  - **minimal approval threshold**: the new value for the minimum approval threshold.
  - **maximal rejection threshold**: the new value for the maximal rejection threshold.
  - **maximal abstention threshold**: the new value for the maximal abstention threshold.
  - **minimal vote threshold**: the new value for the minimal vote threshold.

- **Events**
  - **OrganizationThresholdChanged**
    - **organization_address**: the organization address.
    - **proposer_release_threshold**: the new release threshold.

### **ChangeOrganizationProposerWhiteList**

```Protobuf
rpc ChangeOrganizationProposerWhiteList(ProposerWhiteList) returns (google.protobuf.Empty){}

message ProposerWhiteList {
    repeated aelf.Address proposers = 1;
}

message OrganizationWhiteListChanged{
    option (aelf.is_event) = true;
    aelf.Address organization_address = 1;
    ProposerWhiteList proposer_white_list = 2;
}
```

This method overrides the list of whitelisted proposers.

- **ProposerWhiteList**:
  - **proposers**: the new value for the list.

- **Events**
  - **OrganizationWhiteListChanged**
    - **organization_address**: the organization address.
    - **proposer_white_list**: the new proposer whitelist.

### **CreateProposalBySystemContract**

```Protobuf
rpc CreateProposalBySystemContract(CreateProposalBySystemContractInput) returns (aelf.Hash){}

message CreateProposalBySystemContractInput {
    acs3.CreateProposalInput proposal_input = 1;
    aelf.Address origin_proposer = 2;
}

message ProposalCreated{
    option (aelf.is_event) = true;
    aelf.Hash proposal_id = 1;
}
```

Used by system contracts to create proposals.

- **CreateProposalBySystemContractInput**
  - **CreateProposalInput**: the parameters of creating a proposal.
  - **origin proposer**: the actor that trigger the call.

note: *for CreateProposalInput see CreateProposal*

- **Returns**
  - **Address**: id of the newly created proposal.

- **Events**
  - **ProposalCreated**
    - **proposal_id**: id of the created proposal.

### **ClearProposal**

```Protobuf
rpc ClearProposal(aelf.Hash) returns (google.protobuf.Empty){}
```

Removes the specified proposal.

- **Hash**: id of the proposal to be cleared.

### **ValidateOrganizationExist**

```Protobuf
rpc ValidateOrganizationExist(aelf.Address) returns (google.protobuf.BoolValue){}
```

Checks the existence of an organization.

- **Address**: organization address to be checked.

- **Returns**
  - **BoolValue**: indicates whether the organization exists.

## **View methods**

### **GetOrganization**

```Protobuf
rpc GetOrganization (aelf.Address) returns (Organization){}

message Organization {
    bool proposer_authority_required = 1;
    aelf.Address organization_address = 2;
    aelf.Hash organization_hash = 3;
    acs3.ProposalReleaseThreshold proposal_release_threshold = 4;
    bool parliament_member_proposing_allowed = 5;
}
```

Returns the organization with the provided organization address.

- **Address**: organization address.

- **Returns**
  - **Organization**
    - **proposer authority required**: indicates if proposals need authority to be created.
    - **organization_address**: organization address.
    - **organization hash**: organization id.
    - **ProposalReleaseThreshold**: the threshold.
    - **parliament member proposing allowed**: indicates if parliament member can propose to this organization.

note: *for ProposalReleaseThreshold see CreateOrganization*

### **GetDefaultOrganizationAddress**

```Protobuf
rpc GetDefaultOrganizationAddress (google.protobuf.Empty) returns (aelf.Address){}
```

- **Returns**
  - **Address**: the address of the default organization.

### **ValidateAddressIsParliamentMember**

```Protobuf
rpc ValidateAddressIsParliamentMember(aelf.Address) returns (google.protobuf.BoolValue){}
```

Validates if the provided address is a parliament member.

- **Address**: parliament member address to be checked.

- **Returns**
  - **BoolValue**: indicates whether provided address is one of parliament members.

### **GetProposerWhiteList**

```Protobuf
rpc GetProposerWhiteList(google.protobuf.Empty) returns (acs3.ProposerWhiteList){}

message ProposerWhiteList {
    repeated aelf.Address proposers = 1;
}
```

Returns a list of whitelisted proposers.

- **Returns**
  - **proposers**: the whitelisted proposers.

### **GetNotVotedPendingProposals**

```Protobuf
rpc GetNotVotedPendingProposals(ProposalIdList) returns (ProposalIdList) { }
message ProposalIdList{
    repeated aelf.Hash proposal_ids = 1;
}
```

Filter still pending ones not yet voted by the ``sender`` from provided proposals.

- **ProposalIdList**
  - **proposal ids**: list of proposal id.

- **Returns**
  - **proposal ids**: filtered proposal id list from input ones.


### **GetNotVotedProposals**

```Protobuf
rpc GetNotVotedProposals(ProposalIdList) returns (ProposalIdList){}
message ProposalIdList{
    repeated aelf.Hash proposal_ids = 1;
}
```

Filter not yet voted ones by the ``sender`` from provided proposals.

- **ProposalIdList**
  - **proposal ids**: list of proposal id.

- **Returns**
  - **proposal ids**: filtered proposal id list from input ones.


### **CalculateOrganizationAddress**

```Protobuf
rpc CalculateOrganizationAddress(CreateOrganizationInput) returns (aelf.Address){}

message CreateOrganizationInput {
    acs3.ProposalReleaseThreshold proposal_release_threshold = 1;
    bool proposer_authority_required = 2;
    bool parliament_member_proposing_allowed = 3;
}
```

Calculates with input and returns the organization address.

- **CreateOrganizationInput**
  - **ProposalReleaseThreshold**: the threshold.
  - **proposer authority required**: setting this to true can allow anyone to create proposals.
  - **parliament member proposing allowed**: setting this to true can allow parliament member to create proposals.

note: *for ProposalReleaseThreshold see CreateOrganization*

- **Returns**
  - **Address**: organization address.

### **GetProposal**

```Protobuf
rpc GetProposal(aelf.Hash) returns (ProposalOutput){}

message ProposalOutput {
    aelf.Hash proposal_id = 1;
    string contract_method_name = 2;
    aelf.Address to_address = 3;
    bytes params = 4;
    google.protobuf.Timestamp expired_time = 5;
    aelf.Address organization_address = 6;
    aelf.Address proposer = 7;
    bool to_be_released = 8;
    int64 approval_count = 9;
    int64 rejection_count = 10;
    int64 abstention_count = 11;
}
```

Get the proposal with the given id.

- **Hash**: proposal id.

- **Returns**
  - **proposal id**: id of the proposal.
  - **method name**: the method that this proposal will call when being released.
  - **to address**: the address of the target contract.
  - **params**: the parameters of the release transaction.
  - **expiration**: the date at which this proposal will expire.
  - **organization address**: address of this proposals organization.
  - **proposer**: address of the proposer of this proposal.
  - **to be release**: indicates if this proposal is releasable.
  - **approval count**: approval count for this proposal.
  - **rejection count**: rejection count for this proposal.
  - **abstention count**: abstention count for this proposal.

### **ValidateProposerInWhiteList**

```Protobuf
rpc ValidateProposerInWhiteList(ValidateProposerInWhiteListInput) returns (google.protobuf.BoolValue){}

message ValidateProposerInWhiteListInput {
    aelf.Address proposer = 1;
    aelf.Address organization_address = 2;
}
```

Checks if the proposer is whitelisted.

- **ValidateProposerInWhiteListInput**
  - **proposer**: the address to search/check.
  - **organization address**: address of the organization.

- **Returns**
  - **BoolValue**: indicates whether the proposer is whitelisted.