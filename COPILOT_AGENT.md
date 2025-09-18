## Online Resources
### CheckFileInfo properties documented in Microsoft-365 documents
* https://learn.microsoft.com/en-us/microsoft-365/cloud-storage-partner-program/rest/files/checkfileinfo/checkfileinfo-csppp
* https://learn.microsoft.com/en-us/microsoft-365/cloud-storage-partner-program/rest/files/checkfileinfo?source=recommendations
* https://learn.microsoft.com/en-us/microsoft-365/cloud-storage-partner-program/rest/files/checkfileinfo/checkfileinfo-other

## Updates Made to WOPI Validator

### 1. Enhanced JsonSchemaValidator for Comprehensive CheckFileInfo Validation

The `JsonSchemaValidator` has been significantly enhanced to provide comprehensive validation for all CheckFileInfo properties according to Microsoft's WOPI specification:

**Key Features:**
- **Timestamp Validation**: Validates Unix timestamp properties (`AccessTokenExpiry`, `ServerTime`) to ensure they are within reasonable bounds (current time ± 10 years)
- **Format Validation**: Enforces proper format requirements for properties like `FileExtension` (must start with '.'), `SHA256` (64-character hexadecimal), and URLs
- **Required Property Validation**: Ensures all required properties (`BaseFileName`, `OwnerId`, `Size`, `UserId`, `Version`) are present and non-empty
- **CSPP vs CSPP+ Validation**: Supports different validation rules for Cloud Storage Partner Program and Cloud Storage Partner Program Plus
- **Comprehensive Error Messages**: Provides detailed error messages with specific validation failures and expected ranges

**Timestamp Boundary Validation:**
- For `AccessTokenExpiry` and `ServerTime` properties, the validator now checks that Unix timestamp values are within a 20-year window (10 years before and after current time)
- Invalid timestamps result in descriptive error messages indicating the valid range and the actual timestamp value converted to human-readable format

### 2. Updated CheckFileInfoSchema.json

The JSON schema has been comprehensively updated to include:
- All possible CheckFileInfo properties from the Microsoft documentation
- Proper format constraints (e.g., `FileExtension` pattern, `SHA256` pattern, URL formats)
- Required vs optional property specifications
- Type validation with appropriate constraints (e.g., minimum values for `Size`, `SequenceNumber`)
- Enhanced descriptions for critical properties

**New Properties Added:**
- `SupportsChunkedFileTransfer` for incremental file transfer
- Enhanced validation patterns for existing properties
- Better constraint definitions for numerical properties

### 3. Updated TestCases.xml

All test cases that use `CheckFileInfo` requests now include the `JsonSchemaValidator` with the updated schema:
- **Prerequisites**: All prerequisite test cases now validate the complete CheckFileInfo schema
- **Test Groups**: Schema validation is applied consistently across all test groups
- **Error Detection**: Improved error detection for malformed CheckFileInfo responses

### 4. Enhanced Unit Tests

New comprehensive unit tests have been added to validate:
- **Timestamp Validation**: Tests for valid and invalid Unix timestamps
- **Format Validation**: Tests for proper format requirements (FileExtension, SHA256, etc.)
- **Boundary Conditions**: Tests for edge cases like negative sizes, empty required fields
- **Schema Compliance**: Tests to ensure all required and optional properties are handled correctly

### 5. Benefits for WOPI Implementers

**Improved Validation Coverage:**
- Catches more validation errors early in the development cycle
- Provides specific guidance on what needs to be fixed
- Ensures compliance with Microsoft's latest WOPI specifications

**Better Error Messages:**
- Clear indication of which properties failed validation
- Specific ranges and expected formats provided
- Human-readable timestamp conversion for debugging

**CSPP+ Readiness:**
- Validates properties required for Cloud Storage Partner Program Plus
- Ensures compatibility with collaborative editing features
- Validates timestamp properties critical for token management

### Usage

The enhanced validation is automatically applied to all `CheckFileInfo` requests in the test suite. No changes are required for existing WOPI implementations, but they will now receive more comprehensive validation feedback.

For hosts implementing CSPP+, ensure that:
1. `AccessTokenExpiry` timestamps are valid Unix timestamps within the expected range
2. `UserFriendlyName` is provided when `SupportsCoauth` is true
3. All URL properties use proper absolute URL formats
4. Timestamp properties like `ServerTime` reflect current server time accurately
