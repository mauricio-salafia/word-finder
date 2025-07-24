import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Stress test configuration - aggressive load testing
export const options = {
  stages: [
    { duration: '1m', target: 100 },   // Ramp up to 100 users over 1 minute
    { duration: '2m', target: 100 },   // Stay at 100 users for 2 minutes
    { duration: '1m', target: 200 },   // Ramp up to 200 users over 1 minute
    { duration: '2m', target: 200 },   // Stay at 200 users for 2 minutes
    { duration: '1m', target: 300 },   // Ramp up to 300 users over 1 minute
    { duration: '2m', target: 300 },   // Stay at 300 users for 2 minutes
    { duration: '1m', target: 0 },     // Ramp down to 0 users over 1 minute
  ],
  thresholds: {
    http_req_duration: ['p(95)<3000'], // 95% of requests must complete below 3000ms (more lenient for stress)
    http_req_failed: ['rate<0.05'],    // Error rate must be below 5% (more lenient for stress)
    errors: ['rate<0.05'],             // Custom error rate below 5%
  },
};

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5211';

// Stress test data - more complex scenarios
const stressTestData = {
  // Small matrix - should handle high load
  small: {
    matrix: [
      "abcdc",
      "fgwio", 
      "chill",
      "pqnsd",
      "uvdxy"
    ],
    wordStream: ["chill", "cold", "wind", "snow", "rain", "sun"]
  },
  
  // Medium matrix - moderate complexity
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
      "hello", "world", "test", "cold", "wind", "snow", "rain", "sun",
      "moon", "star", "fire", "water", "earth", "air", "code", "data",
      "quick", "brown", "fox", "jumps", "over", "lazy", "dog"
    ]
  },
  
  // Large matrix - high complexity
  large: {
    matrix: Array.from({ length: 32 }, (_, i) => 
      Array.from({ length: 32 }, (_, j) => {
        // Create more realistic patterns
        const chars = 'abcdefghijklmnopqrstuvwxyz';
        return chars[(i * 7 + j * 3) % 26];
      }).join('')
    ),
    wordStream: [
      "abc", "def", "ghi", "jkl", "mno", "pqr", "stu", "vwx", "yz",
      "hello", "world", "test", "cold", "wind", "snow", "rain", "sun",
      "moon", "star", "fire", "water", "earth", "air", "code", "data",
      "quick", "brown", "fox", "jumps", "over", "lazy", "dog", "the",
      "and", "for", "are", "but", "not", "you", "all", "can", "had",
      "her", "was", "one", "our", "out", "day", "get", "has", "him"
    ]
  },
  
  // Extra large matrix - maximum complexity
  extraLarge: {
    matrix: Array.from({ length: 50 }, (_, i) => 
      Array.from({ length: 50 }, (_, j) => {
        // Create complex patterns that might contain words
        const chars = 'abcdefghijklmnopqrstuvwxyz';
        return chars[(i * 13 + j * 17 + i * j) % 26];
      }).join('')
    ),
    wordStream: [
      "stress", "test", "performance", "benchmark", "load", "high", "traffic",
      "concurrent", "users", "requests", "response", "time", "throughput",
      "scalability", "reliability", "stability", "memory", "cpu", "network",
      "database", "cache", "queue", "worker", "thread", "process", "async",
      "await", "promise", "callback", "event", "handler", "middleware",
      "controller", "service", "repository", "domain", "entity", "value",
      "object", "pattern", "architecture", "design", "clean", "code", "solid"
    ]
  }
};

export default function () {
  // Health check (5% of requests during stress test)
  if (Math.random() < 0.05) {
    const healthResponse = http.get(`${BASE_URL}/api/health`);
    const healthCheck = check(healthResponse, {
      'health check status is 200': (r) => r.status === 200,
      'health check response time < 500ms': (r) => r.timings.duration < 500, // More lenient for stress
    });
    errorRate.add(!healthCheck);
  }

  // Word search with varying complexity (95% of requests)
  const testType = Math.random();
  let payload;
  let expectedMaxDuration;

  if (testType < 0.4) {
    // 40% small matrices
    payload = stressTestData.small;
    expectedMaxDuration = 500;
  } else if (testType < 0.7) {
    // 30% medium matrices
    payload = stressTestData.medium;
    expectedMaxDuration = 1500;
  } else if (testType < 0.9) {
    // 20% large matrices
    payload = stressTestData.large;
    expectedMaxDuration = 3000;
  } else {
    // 10% extra large matrices
    payload = stressTestData.extraLarge;
    expectedMaxDuration = 5000;
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
  });

  errorRate.add(!searchCheck);

  // Reduced sleep time for stress testing
  sleep(Math.random() * 0.5);
}

export function handleSummary(data) {
  return {
    'benchmark-results/stress-test-summary.json': JSON.stringify(data, null, 2),
    'benchmark-results/stress-test-summary.html': generateHtmlReport(data),
  };
}

function generateHtmlReport(data) {
  const metrics = data.metrics;
  return `
<!DOCTYPE html>
<html>
<head>
    <title>WordFinder API - Stress Test Results</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .metric { margin: 10px 0; padding: 10px; border: 1px solid #ddd; }
        .pass { background-color: #d4edda; }
        .fail { background-color: #f8d7da; }
        .warn { background-color: #fff3cd; }
        .summary { font-size: 18px; font-weight: bold; margin: 20px 0; }
        .stress-info { background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin: 20px 0; }
    </style>
</head>
<body>
    <h1>WordFinder API - Stress Test Results</h1>
    <div class="stress-info">
        <strong>Stress Test:</strong> This test pushes the API to its limits with up to 300 concurrent users
        to identify performance bottlenecks and breaking points.
    </div>
    <div class="summary">Test Duration: ${data.state.testRunDurationMs}ms</div>
    
    <h2>Performance Metrics</h2>
    <div class="metric ${metrics.http_req_duration.values.avg < 1000 ? 'pass' : metrics.http_req_duration.values.avg < 2000 ? 'warn' : 'fail'}">
        <strong>HTTP Request Duration (avg):</strong> ${metrics.http_req_duration.values.avg.toFixed(2)}ms
    </div>
    <div class="metric ${metrics.http_req_duration.values['p(95)'] < 3000 ? 'pass' : 'fail'}">
        <strong>HTTP Request Duration (p95):</strong> ${metrics.http_req_duration.values['p(95)'].toFixed(2)}ms
    </div>
    <div class="metric ${metrics.http_req_duration.values['p(99)'] < 5000 ? 'pass' : 'fail'}">
        <strong>HTTP Request Duration (p99):</strong> ${metrics.http_req_duration.values['p(99)'].toFixed(2)}ms
    </div>
    <div class="metric">
        <strong>HTTP Requests per Second:</strong> ${metrics.http_reqs.values.rate.toFixed(2)}
    </div>
    <div class="metric ${metrics.http_req_failed.values.rate < 0.01 ? 'pass' : metrics.http_req_failed.values.rate < 0.05 ? 'warn' : 'fail'}">
        <strong>HTTP Request Failed Rate:</strong> ${(metrics.http_req_failed.values.rate * 100).toFixed(2)}%
    </div>
    
    <h2>Load Characteristics</h2>
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
    
    <h2>Stress Test Analysis</h2>
    <div class="metric">
        <strong>Breaking Point:</strong> 
        ${metrics.http_req_failed.values.rate > 0.1 ? 'API showed signs of stress (>10% error rate)' : 
          metrics.http_req_failed.values.rate > 0.05 ? 'API handled stress well (5-10% error rate)' : 
          'API performed excellently under stress (<5% error rate)'}
    </div>
    <div class="metric">
        <strong>Performance Degradation:</strong> 
        ${metrics.http_req_duration.values.avg > 2000 ? 'Significant performance degradation observed' : 
          metrics.http_req_duration.values.avg > 1000 ? 'Moderate performance degradation observed' : 
          'Minimal performance degradation observed'}
    </div>
</body>
</html>
  `;
}
