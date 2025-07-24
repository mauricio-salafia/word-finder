import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Endurance test configuration - long-running test for stability
export const options = {
  stages: [
    { duration: '2m', target: 50 },    // Ramp up to 50 users over 2 minutes
    { duration: '10m', target: 50 },   // Maintain 50 users for 10 minutes
    { duration: '2m', target: 100 },   // Ramp up to 100 users over 2 minutes
    { duration: '15m', target: 100 },  // Maintain 100 users for 15 minutes
    { duration: '2m', target: 75 },    // Reduce to 75 users over 2 minutes
    { duration: '15m', target: 75 },   // Maintain 75 users for 15 minutes
    { duration: '2m', target: 0 },     // Ramp down to 0 users over 2 minutes
  ],
  thresholds: {
    http_req_duration: ['p(95)<1500'], // 95% of requests must complete below 1500ms
    http_req_failed: ['rate<0.02'],    // Error rate must be below 2%
    errors: ['rate<0.02'],             // Custom error rate below 2%
  },
};

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5211';

// Test data for endurance testing - varied complexity to test memory management
const enduranceTestData = {
  // Small matrix - frequent requests
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
  
  // Medium matrix - moderate load
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
    wordStream: [
      "abc", "def", "ghi", "jkl", "mno", "pqr", "stu", "vwx", "yz",
      "hello", "world", "test", "cold", "wind", "snow", "rain", "sun"
    ]
  },
  
  // Large matrix - memory intensive
  large: {
    matrix: Array.from({ length: 32 }, (_, i) => 
      Array.from({ length: 32 }, (_, j) => {
        // Create realistic patterns for endurance testing
        const chars = 'abcdefghijklmnopqrstuvwxyz';
        return chars[(i * 11 + j * 13) % 26];
      }).join('')
    ),
    wordStream: [
      "endurance", "test", "memory", "leak", "performance", "stability",
      "long", "running", "continuous", "load", "sustained", "duration",
      "reliability", "consistency", "availability", "uptime", "service",
      "quality", "monitoring", "metrics", "health", "status", "check"
    ]
  },
  
  // Variable matrix - different each time to test memory allocation
  variable: function() {
    const size = 8 + Math.floor(Math.random() * 8); // 8-16 size
    return {
      matrix: Array.from({ length: size }, (_, i) => 
        Array.from({ length: size }, (_, j) => {
          const chars = 'abcdefghijklmnopqrstuvwxyz';
          return chars[(i * 7 + j * 17 + Date.now()) % 26];
        }).join('')
      ),
      wordStream: [
        "variable", "dynamic", "random", "memory", "allocation", "test",
        "garbage", "collection", "heap", "stack", "buffer", "pool"
      ]
    };
  }
};

export default function () {
  // Health check (regular monitoring during endurance test)
  if (Math.random() < 0.08) {
    const healthResponse = http.get(`${BASE_URL}/api/health`);
    const healthCheck = check(healthResponse, {
      'health check status is 200': (r) => r.status === 200,
      'health check response time < 300ms': (r) => r.timings.duration < 300,
      'health check has status field': (r) => r.json('status') === 'Healthy',
      'health check has timestamp': (r) => r.json('timestamp') !== undefined,
    });
    errorRate.add(!healthCheck);
  }

  // Word search with emphasis on varied data for memory testing
  const testType = Math.random();
  let payload;
  let expectedMaxDuration;

  if (testType < 0.5) {
    // 50% small matrices - frequent, light requests
    payload = enduranceTestData.small;
    expectedMaxDuration = 400;
  } else if (testType < 0.8) {
    // 30% medium matrices - moderate load
    payload = enduranceTestData.medium;
    expectedMaxDuration = 1000;
  } else if (testType < 0.95) {
    // 15% large matrices - heavy load
    payload = enduranceTestData.large;
    expectedMaxDuration = 2000;
  } else {
    // 5% variable matrices - memory allocation testing
    payload = enduranceTestData.variable();
    expectedMaxDuration = 1500;
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
    'search no timeout errors': (r) => r.status !== 408,
    'search no memory errors': (r) => r.status !== 503,
  });

  errorRate.add(!searchCheck);

  // Realistic user behavior - longer think time for endurance
  sleep(1 + Math.random() * 3);
}

export function handleSummary(data) {
  return {
    'benchmark-results/endurance-test-summary.json': JSON.stringify(data, null, 2),
    'benchmark-results/endurance-test-summary.html': generateHtmlReport(data),
  };
}

function generateHtmlReport(data) {
  const metrics = data.metrics;
  const testDurationHours = (data.state.testRunDurationMs / 1000 / 60 / 60).toFixed(2);
  
  return `
<!DOCTYPE html>
<html>
<head>
    <title>WordFinder API - Endurance Test Results</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .metric { margin: 10px 0; padding: 10px; border: 1px solid #ddd; }
        .pass { background-color: #d4edda; }
        .fail { background-color: #f8d7da; }
        .warn { background-color: #fff3cd; }
        .summary { font-size: 18px; font-weight: bold; margin: 20px 0; }
        .endurance-info { background-color: #f0f8ff; padding: 15px; border-left: 4px solid #4CAF50; margin: 20px 0; }
        .stability-metrics { background-color: #f9f9f9; padding: 15px; margin: 20px 0; }
    </style>
</head>
<body>
    <h1>WordFinder API - Endurance Test Results</h1>
    <div class="endurance-info">
        <strong>Endurance Test:</strong> This test runs for extended periods (${testDurationHours} hours) with sustained load
        to detect memory leaks, performance degradation, and stability issues over time.
    </div>
    <div class="summary">Test Duration: ${testDurationHours} hours (${data.state.testRunDurationMs}ms)</div>
    
    <h2>Performance Metrics</h2>
    <div class="metric ${metrics.http_req_duration.values.avg < 800 ? 'pass' : metrics.http_req_duration.values.avg < 1200 ? 'warn' : 'fail'}">
        <strong>HTTP Request Duration (avg):</strong> ${metrics.http_req_duration.values.avg.toFixed(2)}ms
    </div>
    <div class="metric ${metrics.http_req_duration.values['p(95)'] < 1500 ? 'pass' : 'fail'}">
        <strong>HTTP Request Duration (p95):</strong> ${metrics.http_req_duration.values['p(95)'].toFixed(2)}ms
    </div>
    <div class="metric ${metrics.http_req_duration.values['p(99)'] < 2500 ? 'pass' : 'warn'}">
        <strong>HTTP Request Duration (p99):</strong> ${metrics.http_req_duration.values['p(99)'].toFixed(2)}ms
    </div>
    <div class="metric">
        <strong>HTTP Requests per Second:</strong> ${metrics.http_reqs.values.rate.toFixed(2)}
    </div>
    <div class="metric ${metrics.http_req_failed.values.rate < 0.01 ? 'pass' : metrics.http_req_failed.values.rate < 0.02 ? 'warn' : 'fail'}">
        <strong>HTTP Request Failed Rate:</strong> ${(metrics.http_req_failed.values.rate * 100).toFixed(2)}%
    </div>
    
    <h2>Endurance Characteristics</h2>
    <div class="metric">
        <strong>Max Virtual Users:</strong> ${metrics.vus_max.values.max}
    </div>
    <div class="metric">
        <strong>Total Requests:</strong> ${metrics.http_reqs.values.count}
    </div>
    <div class="metric">
        <strong>Average Requests per Hour:</strong> ${(metrics.http_reqs.values.count / testDurationHours).toFixed(0)}
    </div>
    <div class="metric">
        <strong>Data Received:</strong> ${(metrics.data_received.values.count / 1024 / 1024).toFixed(2)} MB
    </div>
    <div class="metric">
        <strong>Data Sent:</strong> ${(metrics.data_sent.values.count / 1024 / 1024).toFixed(2)} MB
    </div>
    
    <div class="stability-metrics">
        <h2>Stability Analysis</h2>
        <div class="metric">
            <strong>Memory Stability:</strong> 
            ${metrics.http_req_failed.values.rate < 0.01 ? 'Excellent - No signs of memory leaks' : 
              metrics.http_req_failed.values.rate < 0.02 ? 'Good - Stable memory usage' : 
              'Concerning - Potential memory issues detected'}
        </div>
        <div class="metric">
            <strong>Performance Stability:</strong> 
            ${metrics.http_req_duration.values.avg < 800 ? 'Excellent - Consistent performance over time' : 
              metrics.http_req_duration.values.avg < 1200 ? 'Good - Stable performance' : 
              'Poor - Performance degradation detected'}
        </div>
        <div class="metric">
            <strong>Service Reliability:</strong> 
            ${metrics.http_req_failed.values.rate < 0.005 ? 'Excellent - Highly reliable service' : 
              metrics.http_req_failed.values.rate < 0.02 ? 'Good - Reliable service' : 
              'Poor - Service reliability concerns'}
        </div>
    </div>
    
    <h2>Endurance Test Recommendations</h2>
    <div class="metric">
        <strong>Production Readiness:</strong> 
        ${metrics.http_req_failed.values.rate < 0.01 && metrics.http_req_duration.values.avg < 800 ? 
          'API is production-ready for long-running operations' : 
          metrics.http_req_failed.values.rate < 0.02 && metrics.http_req_duration.values.avg < 1200 ? 
          'API shows good endurance but monitor for potential issues' : 
          'API needs stability improvements before production deployment'}
    </div>
    <div class="metric">
        <strong>Monitoring Focus:</strong> 
        Monitor memory usage, response times, and error rates during extended operations. 
        ${metrics.http_req_duration.values.avg > 1000 ? 'Pay special attention to performance degradation over time.' : 
          'Current performance levels are stable for extended operations.'}
    </div>
</body>
</html>
  `;
}
