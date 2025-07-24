#!/bin/bash

# WordFinder API Test Script
# This script tests the WordFinder API endpoints
#
# Updated: Health endpoint moved from /api/wordfinder/health to /api/health
# due to separation of concerns (HealthController vs WordFinderController)

API_BASE_URL="http://localhost:5211"

echo "=== WordFinder API Test ==="
echo

# Test 1: Health Check
echo "1. Testing Health Check endpoint..."
curl -s "${API_BASE_URL}/api/health" | jq '.' 2>/dev/null || echo "Health check failed or jq not available"
echo
echo

# Test 1.1: Verify old health endpoint is no longer available
echo "1.1. Verifying old health endpoint returns 404..."
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "${API_BASE_URL}/api/wordfinder/health")
if [ "$HTTP_STATUS" = "404" ]; then
    echo "✅ Old health endpoint correctly returns 404"
else
    echo "⚠️  Old health endpoint returned status: $HTTP_STATUS (expected 404)"
fi
echo
echo

# Test 2: Word Search with Example from Challenge
echo "2. Testing Word Search with challenge example..."
curl -s -X POST "${API_BASE_URL}/api/wordfinder/search" \
  -H "Content-Type: application/json" \
  -d '{
    "matrix": [
      "abcdc",
      "fgwio", 
      "chill",
      "pqnsd",
      "uvdxy"
    ],
    "wordStream": ["chill", "cold", "wind", "snow"]
  }' | jq '.' 2>/dev/null || echo "Word search test failed or jq not available"
echo
echo

# Test 3: Performance Test with Large Matrix
echo "3. Testing Performance with larger matrix..."
curl -s -X POST "${API_BASE_URL}/api/wordfinder/search" \
  -H "Content-Type: application/json" \
  -d '{
    "matrix": [
      "abcdefghijklmnop",
      "bcdefghijklmnopa",
      "cdefghijklmnopab",
      "defghijklmnopabb",
      "efghijklmnopabcd",
      "fghijklmnopabcde",
      "ghijklmnopabcdef",
      "hijklmnopabcdefg"
    ],
    "wordStream": ["abc", "def", "ghi", "jkl", "mno", "hello", "world", "test"]
  }' | jq '.' 2>/dev/null || echo "Performance test failed or jq not available"
echo
echo

echo "=== Tests Complete ==="
