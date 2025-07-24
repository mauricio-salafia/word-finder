import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Spike test configuration - sudden traffic spikes
export const options = {
  stages: [
    { duration: '30s', target: 20 },   // Normal load
    { duration: '10s', target: 200 },  // Spike to 200 users in 10 seconds
    { duration: '30s', target: 200 },  // Maintain spike for 30 seconds
    { duration: '10s', target: 20 },   // Drop back to normal
    { duration: '30s', target: 20 },   // Maintain normal load
    { duration: '10s', target: 300 },  // Bigger spike to 300 users
    { duration: '30s', target: 300 },  // Maintain bigger spike
    { duration: '10s', target: 20 },   // Drop back to normal
    { duration: '30s', target: 20 },   // Recovery period
    { duration: '10s', target: 0 },    // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000'], // 95% of requests must complete below 2000ms
    http_req_failed: ['rate<0.03'],    // Error rate must be below 3%
    errors: ['rate<0.03'],             // Custom error rate below 3%
  },
};

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5211';

// Test data optimized for spike testing
const spikeTestData = {
  // Fast processing data for spike handling
  quick: {
    matrix: [
      "abcdc",
      "fgwio", 
      "chill",
      "pqnsd",
      "uvdxy"
    ],
    wordStream: ["chill", "cold", "wind"]
  },
  
  // Medium complexity
  medium: {
    matrix: [
      "abcdefghij",
      "bcdefghijk",
      "cdefghijkl",
      "defghijklm",
      "efghijklmn",
      "fghijklmno",
      "ghijklmnop",
      "hijklmnopq",
      "ijklmnopqr",
      "jklmnopqrs"
    ],
    wordStream: ["abc", "def", "ghi", "jkl", "mno", "hello", "world", "test", "cold", "wind"]
  },
  
  // Complex data to test spike handling under load
  complex: {
    matrix: Array.from({ length: 20 }, (_, i) => 
      Array.from({ length: 20 }, (_, j) => {
        // Create patterns that might contain common words
        const chars = 'abcdefghijklmnopqrstuvwxyz';
        return chars[(i * 5 + j * 7) % 26];
      }).join('')
    ),
    wordStream: [
      "test", "spike", "load", "performance", "quick", "fast", "slow",
      "hello", "world", "code", "data", "word", "find", "search",
      "matrix", "stream", "api", "http", "json", "response", "time"
    ]
  }
};

export default function () {
  // Health check (minimal during spike test to focus on main functionality)
  if (Math.random() < 0.02) {
    const healthResponse = http.get(`${BASE_URL}/api/health`);
    const healthCheck = check(healthResponse, {
      'health check status is 200': (r) => r.status === 200,
      'health check response time < 200ms': (r) => r.timings.duration < 200,
    });
    errorRate.add(!healthCheck);
  }

  // Word search - optimized for spike testing
  const testType = Math.random();
  let payload;
  let expectedMaxDuration;

  if (testType < 0.6) {
    // 60% quick processing requests
    payload = spikeTestData.quick;
    expectedMaxDuration = 300;
  } else if (testType < 0.9) {
    // 30% medium complexity
    payload = spikeTestData.medium;
    expectedMaxDuration = 800;
  } else {
    // 10% complex requests
    payload = spikeTestData.complex;
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
    'search foundWords max 10 items': (r) => r.json('foundWords').length <= 10,
    'search no server errors': (r) => r.status !== 500,
  });

  errorRate.add(!searchCheck);

  // Minimal sleep to maximize spike impact
  sleep(Math.random() * 0.3);
}

export function handleSummary(data) {
  return {
    'benchmark-results/spike-test-summary.json': JSON.stringify(data, null, 2),
    'benchmark-results/spike-test-summary.html': generateHtmlReport(data),
  };
}

function generateHtmlReport(data) {
  const metrics = data.metrics;
  return `
<!DOCTYPE html>
<html>
<head>
    <title>WordFinder API - Spike Test Results</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .metric { margin: 10px 0; padding: 10px; border: 1px solid #ddd; }
        .pass { background-color: #d4edda; }
        .fail { background-color: #f8d7da; }
        .warn { background-color: #fff3cd; }
        .summary { font-size: 18px; font-weight: bold; margin: 20px 0; }
        .spike-info { background-color: #e7f3ff; padding: 15px; border-left: 4px solid #2196F3; margin: 20px 0; }
    </style>
</head>
<body>
    <h1>WordFinder API - Spike Test Results</h1>
    <div class="spike-info">
        <strong>Spike Test:</strong> This test simulates sudden traffic spikes from 20 to 300 users
        to test the API's ability to handle sudden load increases and recovery.
    </div>
    <div class="summary">Test Duration: ${data.state.testRunDurationMs}ms</div>
    
    <h2>Performance Metrics</h2>
    <div class="metric ${metrics.http_req_duration.values.avg < 800 ? 'pass' : metrics.http_req_duration.values.avg < 1500 ? 'warn' : 'fail'}">
        <strong>HTTP Request Duration (avg):</strong> ${metrics.http_req_duration.values.avg.toFixed(2)}ms
    </div>
    <div class="metric ${metrics.http_req_duration.values['p(95)'] < 2000 ? 'pass' : 'fail'}">
        <strong>HTTP Request Duration (p95):</strong> ${metrics.http_req_duration.values['p(95)'].toFixed(2)}ms
    </div>
    <div class="metric ${metrics.http_req_duration.values['p(99)'] < 3000 ? 'pass' : 'warn'}">
        <strong>HTTP Request Duration (p99):</strong> ${metrics.http_req_duration.values['p(99)'].toFixed(2)}ms
    </div>
    <div class="metric">
        <strong>HTTP Requests per Second:</strong> ${metrics.http_reqs.values.rate.toFixed(2)}
    </div>
    <div class="metric ${metrics.http_req_failed.values.rate < 0.01 ? 'pass' : metrics.http_req_failed.values.rate < 0.03 ? 'warn' : 'fail'}">
        <strong>HTTP Request Failed Rate:</strong> ${(metrics.http_req_failed.values.rate * 100).toFixed(2)}%
    </div>
    
    <h2>Spike Characteristics</h2>
    <div class="metric">
        <strong>Max Virtual Users:</strong> ${metrics.vus_max.values.max}
    </div>
    <div class="metric">
        <strong>Total Requests:</strong> ${metrics.http_reqs.values.count}
    </div>
    <div class="metric">
        <strong>Data Received:</strong> ${(metrics.data_received.values.count / 1024 / 1024).toFixed(2)} MB
    </div>
    <div class="metric">
        <strong>Data Sent:</strong> ${(metrics.data_sent.values.count / 1024 / 1024).toFixed(2)} MB
    </div>
    
    <h2>Spike Test Analysis</h2>
    <div class="metric">
        <strong>Spike Resilience:</strong> 
        ${metrics.http_req_failed.values.rate < 0.01 ? 'Excellent - API handled spikes with minimal errors' : 
          metrics.http_req_failed.values.rate < 0.03 ? 'Good - API handled spikes well with acceptable error rate' : 
          'Poor - API struggled with traffic spikes'}
    </div>
    <div class="metric">
        <strong>Recovery Performance:</strong> 
        ${metrics.http_req_duration.values.avg < 800 ? 'Excellent - Fast recovery after spikes' : 
          metrics.http_req_duration.values.avg < 1500 ? 'Good - Reasonable recovery time' : 
          'Poor - Slow recovery after spikes'}
    </div>
    <div class="metric">
        <strong>Recommendation:</strong> 
        ${metrics.http_req_failed.values.rate < 0.01 && metrics.http_req_duration.values.avg < 800 ? 
          'API is production-ready for traffic spikes' : 
          metrics.http_req_failed.values.rate < 0.03 && metrics.http_req_duration.values.avg < 1500 ? 
          'API handles spikes well but consider auto-scaling optimizations' : 
          'API needs optimization for spike handling - consider caching, rate limiting, or scaling improvements'}
    </div>
</body>
</html>
  `;
}
