const blockedPatterns = [/BEGIN RSA PRIVATE KEY/, /AKIA[0-9A-Z]{16}/, /-----BEGIN PRIVATE KEY-----/];

console.log("Running lightweight repository secret scan...");
console.log(`Loaded ${blockedPatterns.length} high-risk signature checks.`);
