# Store your Gemini API key securely. It is not recommended to hardcode it.
# A better practice is to use an environment variable.
# For a quick test, you can paste it here, but delete it after.
$API_KEY = "AIzaSyBC1C6p6f7IVg0CgoX6dBiCqnm80jc3IfM"

# The URL for the Gemini API endpoint
$uri = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=$API_KEY"

# The headers for the request
$headers = @{
    "Content-Type" = "application/json"
}

# The body of the request, which contains the prompt.
# It's a JSON object with the 'contents' and 'parts' structure.
$body = @{
    contents = @(
        @{
            parts = @(
                @{
                    text = "Explain how AI works in a single paragraph."
                }
            )
        }
    )
} | ConvertTo-Json

# Use Invoke-RestMethod to send the request
try {
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body

    # Extract the text from the response
    # The response is a JSON object, so we navigate through its structure.
    $generatedText = $response.candidates[0].content.parts[0].text

    Write-Host "--- Gemini API Response ---"
    Write-Host $generatedText

} catch {
    Write-Host "An error occurred while calling the Gemini API."
    Write-Host $_.Exception.Response.StatusCode
    Write-Host $_.Exception.Response.Content
}