import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Test configuration
export const options = {
  stages: [
    { duration: '30s', target: 10 },  // Ramp up to 10 users over 30 seconds
    { duration: '1m', target: 10 },   // Stay at 10 users for 1 minute
    { duration: '30s', target: 25 },  // Ramp up to 25 users over 30 seconds
    { duration: '2m', target: 25 },   // Stay at 25 users for 2 minutes
    { duration: '30s', target: 0 },   // Ramp down to 0 users over 30 seconds
  ],
  thresholds: {
    http_req_duration: ['p(95)<1000'], // 95% of requests must complete below 1000ms
    http_req_failed: ['rate<0.01'],    // Error rate must be below 1%
    errors: ['rate<0.01'],             // Custom error rate below 1%
  },
};

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5211';

// Test data sets
const testData = {
  small: {
    matrix: [
      "abcdc",
      "fgwio", 
      "chill",
      "pqnsd",
      "uvdxy"
    ],
    wordStream: ["chill", "cold", "wind", "snow"]
  },
  medium: {
    matrix: [
      "abcdefghijklmnop",
      "bcdefghijklmnopa",
      "cdefghijklmnopab",
      "defghijklmnopabs",
      "efghijklmnopabcd",
      "fghijklmnopabcde",
      "ghijklmnopabcdef",
      "hijklmnopabcdefg",
      "ijklmnopabcdefgh",
      "jklmnopabcdefghi",
      "klmnopabcdefghij",
      "lmnopabcdefghijk",
      "mnopabcdefghijkl",
      "nopabcdefghijklm",
      "opabcdefghijklmn",
      "pabcdefghijklmno"
    ],
    wordStream: ["abc", "def", "ghi", "jkl", "mno", "pqr", "stu", "vwx", "hello", "world", "test", "cold", "wind"]
  },
  large: {
    matrix: Array.from({ length: 32 }, (_, i) => 
      Array.from({ length: 32 }, (_, j) => 
        String.fromCharCode(97 + ((i + j) % 26))
      ).join('')
    ),
    wordStream: [
      "abc", "def", "ghi", "jkl", "mno", "pqr", "stu", "vwx", "yz",
      "hello", "world", "test", "cold", "wind", "snow", "rain", "sun",
      "moon", "star", "fire", "water", "earth", "air", "code", "data"
    ]
  }
};

export default function () {
  // Test 1: Health Check (10% of requests)
  if (Math.random() < 0.1) {
    const healthResponse = http.get(`${BASE_URL}/api/health`);
    const healthCheck = check(healthResponse, {
      'health check status is 200': (r) => r.status === 200,
      'health check response time < 100ms': (r) => r.timings.duration < 100,
      'health check has status field': (r) => r.json('status') === 'Healthy',
    });
    errorRate.add(!healthCheck);
  }

  // Test 2: Word Search with different matrix sizes
  const testType = Math.random();
  let payload;
  let expectedMaxDuration;

  if (testType < 0.6) {
    // 60% small matrices
    payload = testData.small;
    expectedMaxDuration = 200;
  } else if (testType < 0.9) {
    // 30% medium matrices
    payload = testData.medium;
    expectedMaxDuration = 500;
  } else {
    // 10% large matrices
    payload = testData.large;
    expectedMaxDuration = 2000;
  }

  const searchResponse = http.post(`${BASE_URL}/api/wordfinder/search`, JSON.stringify(payload), {
    headers: {
      'Content-Type': 'application/json',
    },
  });

  const searchCheck = check(searchResponse, {
    'search status is 200': (r) => r.status === 200,
    [`search response time < ${expectedMaxDuration}ms`]: (r) => r.timings.duration < expectedMaxDuration,
    'search response has foundWords': (r) => r.json('foundWords') !== undefined,
    'search response has totalWordsSearched': (r) => r.json('totalWordsSearched') !== undefined,
    'search response has processingTimeMs': (r) => r.json('processingTimeMs') !== undefined,
    'search foundWords is array': (r) => Array.isArray(r.json('foundWords')),
    'search totalWordsSearched is number': (r) => typeof r.json('totalWordsSearched') === 'number',
    'search processingTimeMs is number': (r) => typeof r.json('processingTimeMs') === 'number',
  });

  errorRate.add(!searchCheck);

  // Small random sleep to simulate real user behavior
  sleep(Math.random() * 2);
}

export function handleSummary(data) {
  return {
    'benchmark-results/basic-load-test-summary.json': JSON.stringify(data, null, 2),
    'benchmark-results/basic-load-test-summary.html': generateHtmlReport(data),
  };
}

function generateHtmlReport(data) {
  const metrics = data.metrics;
  return `
<!DOCTYPE html>
<html>
<head>
    <title>WordFinder API - Basic Load Test Results</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .metric { margin: 10px 0; padding: 10px; border: 1px solid #ddd; }
        .pass { background-color: #d4edda; }
        .fail { background-color: #f8d7da; }
        .summary { font-size: 18px; font-weight: bold; margin: 20px 0; }
    </style>
</head>
<body>
    <h1>WordFinder API - Basic Load Test Results</h1>
    <div class="summary">Test Duration: ${data.state.testRunDurationMs}ms</div>
    
    <h2>Performance Metrics</h2>
    <div class="metric">
        <strong>HTTP Request Duration (avg):</strong> ${metrics.http_req_duration.values.avg.toFixed(2)}ms
    </div>
    <div class="metric">
        <strong>HTTP Request Duration (p95):</strong> ${metrics.http_req_duration.values['p(95)'].toFixed(2)}ms
    </div>
    <div class="metric">
        <strong>HTTP Requests per Second:</strong> ${metrics.http_reqs.values.rate.toFixed(2)}
    </div>
    <div class="metric ${metrics.http_req_failed.values.rate < 0.01 ? 'pass' : 'fail'}">
        <strong>HTTP Request Failed Rate:</strong> ${(metrics.http_req_failed.values.rate * 100).toFixed(2)}%
    </div>
    
    <h2>Virtual Users</h2>
    <div class="metric">
        <strong>Max Virtual Users:</strong> ${metrics.vus_max.values.max}
    </div>
    
    <h2>Test Summary</h2>
    <div class="metric">
        <strong>Total Requests:</strong> ${metrics.http_reqs.values.count}
    </div>
    <div class="metric">
        <strong>Data Received:</strong> ${(metrics.data_received.values.count / 1024 / 1024).toFixed(2)} MB
    </div>
    <div class="metric">
        <strong>Data Sent:</strong> ${(metrics.data_sent.values.count / 1024 / 1024).toFixed(2)} MB
    </div>
</body>
</html>
  `;
}
