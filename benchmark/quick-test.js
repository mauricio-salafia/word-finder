import http from 'k6/http';
import { check } from 'k6';
import { TestData, CommonChecks } from './config.js';

// Quick test configuration - minimal load for validation
export const options = {
  stages: [
    { duration: '10s', target: 5 },   // Ramp up to 5 users
    { duration: '30s', target: 5 },   // Stay at 5 users
    { duration: '10s', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<1000'],
    http_req_failed: ['rate<0.05'],
  },
};

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5211';

export default function () {
  // Test 1: Health Check
  const healthResponse = http.get(`${BASE_URL}/api/health`);
  const healthCheck = CommonChecks.healthCheck(healthResponse);
  
  // Test 2: Basic Word Search
  const searchPayload = TestData.challenge;
  const searchResponse = http.post(`${BASE_URL}/api/wordfinder/search`, JSON.stringify(searchPayload), {
    headers: { 'Content-Type': 'application/json' },
  });
  
  const searchCheck = CommonChecks.wordSearchResponse(searchResponse, 500);
  
  // Test 3: Validate expected results
  if (searchResponse.status === 200) {
    const results = searchResponse.json();
    check(results, {
      'found expected chill': (r) => r.foundWords.includes('chill'),
      'found expected cold': (r) => r.foundWords.includes('cold'),
      'found expected wind': (r) => r.foundWords.includes('wind'),
      'did not find snow': (r) => !r.foundWords.includes('snow'),
      'total words is 4': (r) => r.totalWordsSearched === 4,
      'processing time reasonable': (r) => r.processingTimeMs < 100,
    });
  }
}

export function handleSummary(data) {
  const passed = data.metrics.http_req_failed.values.rate < 0.05;
  const avgDuration = data.metrics.http_req_duration.values.avg;
  
  console.log(`\n=== Quick Test Summary ===`);
  console.log(`Status: ${passed ? '✅ PASSED' : '❌ FAILED'}`);
  console.log(`Average Response Time: ${avgDuration.toFixed(2)}ms`);
  console.log(`Error Rate: ${(data.metrics.http_req_failed.values.rate * 100).toFixed(2)}%`);
  console.log(`Total Requests: ${data.metrics.http_reqs.values.count}`);
  
  if (!passed) {
    console.log(`\n❌ Quick test failed - API may not be ready for full benchmarks`);
  } else {
    console.log(`\n✅ Quick test passed - API is ready for full benchmarks`);
  }
  
  return {
    'benchmark-results/quick-test-summary.json': JSON.stringify(data, null, 2),
  };
}
