// K6 Configuration for WordFinder API Benchmarks
// This file contains shared configuration and utilities for all benchmark tests

import { check } from 'k6';

// Common test data that can be reused across all benchmark tests
export const TestData = {
  // Challenge example data
  challenge: {
    matrix: [
      "abcdc",
      "fgwio", 
      "chill",
      "pqnsd",
      "uvdxy"
    ],
    wordStream: ["chill", "cold", "wind", "snow"],
    expectedWords: ["chill", "cold", "wind"], // Based on algorithm analysis
    expectedProcessingTime: 50 // Expected processing time in ms
  },

  // Small matrices for high-frequency testing
  small: [
    {
      matrix: ["abc", "def", "ghi"],
      wordStream: ["abc", "def", "ghi", "xyz"],
      maxProcessingTime: 100
    },
    {
      matrix: ["hello", "world", "tests"],
      wordStream: ["hello", "world", "test", "words"],
      maxProcessingTime: 100
    }
  ],

  // Medium matrices for balanced testing
  medium: [
    {
      matrix: Array.from({ length: 16 }, (_, i) => 
        Array.from({ length: 16 }, (_, j) => 
          String.fromCharCode(97 + ((i + j) % 26))
        ).join('')
      ),
      wordStream: [
        "abc", "def", "ghi", "jkl", "mno", "pqr", "stu", "vwx",
        "hello", "world", "test", "code", "data", "find", "search"
      ],
      maxProcessingTime: 500
    }
  ],

  // Large matrices for stress testing
  large: [
    {
      matrix: Array.from({ length: 32 }, (_, i) => 
        Array.from({ length: 32 }, (_, j) => 
          String.fromCharCode(97 + ((i * 7 + j * 13) % 26))
        ).join('')
      ),
      wordStream: [
        "performance", "benchmark", "testing", "matrix", "search", "algorithm",
        "optimization", "scalability", "reliability", "throughput", "latency",
        "memory", "cpu", "network", "database", "cache", "async", "await"
      ],
      maxProcessingTime: 2000
    }
  ],

  // Extra large matrices for extreme testing
  extraLarge: [
    {
      matrix: Array.from({ length: 64 }, (_, i) => 
        Array.from({ length: 64 }, (_, j) => 
          String.fromCharCode(97 + ((i * 11 + j * 17) % 26))
        ).join('')
      ),
      wordStream: [
        "stress", "test", "extreme", "load", "maximum", "capacity", "limit",
        "breaking", "point", "endurance", "stability", "consistency", "quality",
        "service", "level", "agreement", "monitoring", "alerting", "metrics"
      ],
      maxProcessingTime: 5000
    }
  ]
};

// Common performance thresholds
export const PerformanceThresholds = {
  // Response time thresholds (milliseconds)
  responseTime: {
    excellent: 100,
    good: 500,
    acceptable: 1000,
    poor: 2000
  },
  
  // Error rate thresholds (percentage)
  errorRate: {
    excellent: 0.1,
    good: 1.0,
    acceptable: 2.0,
    poor: 5.0
  },
  
  // Throughput thresholds (requests per second)
  throughput: {
    minimum: 50,
    good: 200,
    excellent: 500
  }
};

// Common check functions
export const CommonChecks = {
  // Basic response validation
  basicResponse: (response) => {
    return check(response, {
      'status is 200': (r) => r.status === 200,
      'response time < 10s': (r) => r.timings.duration < 10000,
      'response has body': (r) => r.body.length > 0,
    });
  },

  // Health check validation
  healthCheck: (response) => {
    return check(response, {
      'health status is 200': (r) => r.status === 200,
      'health response time < 500ms': (r) => r.timings.duration < 500,
      'health has status field': (r) => r.json('status') === 'Healthy',
      'health has timestamp': (r) => r.json('timestamp') !== undefined,
    });
  },

  // Word search response validation
  wordSearchResponse: (response, maxDuration = 2000) => {
    return check(response, {
      'search status is 200': (r) => r.status === 200,
      [`search response time < ${maxDuration}ms`]: (r) => r.timings.duration < maxDuration,
      'search response has foundWords': (r) => r.json('foundWords') !== undefined,
      'search response has totalWordsSearched': (r) => r.json('totalWordsSearched') !== undefined,
      'search response has processingTimeMs': (r) => r.json('processingTimeMs') !== undefined,
      'search foundWords is array': (r) => Array.isArray(r.json('foundWords')),
      'search totalWordsSearched is number': (r) => typeof r.json('totalWordsSearched') === 'number',
      'search processingTimeMs is number': (r) => typeof r.json('processingTimeMs') === 'number',
      'search foundWords max 10 items': (r) => r.json('foundWords').length <= 10,
      'search totalWordsSearched >= 0': (r) => r.json('totalWordsSearched') >= 0,
      'search processingTimeMs >= 0': (r) => r.json('processingTimeMs') >= 0,
    });
  },

  // Error response validation
  errorResponse: (response, expectedStatus) => {
    return check(response, {
      [`status is ${expectedStatus}`]: (r) => r.status === expectedStatus,
      'error response has body': (r) => r.body.length > 0,
      'error response time < 1000ms': (r) => r.timings.duration < 1000,
    });
  }
};

// Utility functions
export const Utils = {
  // Get random test data
  getRandomTestData: (type = 'mixed') => {
    switch (type) {
      case 'small':
        return TestData.small[Math.floor(Math.random() * TestData.small.length)];
      case 'medium':
        return TestData.medium[Math.floor(Math.random() * TestData.medium.length)];
      case 'large':
        return TestData.large[Math.floor(Math.random() * TestData.large.length)];
      case 'extraLarge':
        return TestData.extraLarge[Math.floor(Math.random() * TestData.extraLarge.length)];
      case 'challenge':
        return TestData.challenge;
      default: // mixed
        const types = ['small', 'medium', 'large'];
        const randomType = types[Math.floor(Math.random() * types.length)];
        return Utils.getRandomTestData(randomType);
    }
  },

  // Generate dynamic test data
  generateDynamicMatrix: (size = 8, seed = Date.now()) => {
    return {
      matrix: Array.from({ length: size }, (_, i) => 
        Array.from({ length: size }, (_, j) => {
          const chars = 'abcdefghijklmnopqrstuvwxyz';
          return chars[(i * 7 + j * 13 + seed) % 26];
        }).join('')
      ),
      wordStream: [
        "dynamic", "test", "data", "random", "matrix", "generated",
        "performance", "benchmark", "validation", "check"
      ],
      maxProcessingTime: size * size * 2 // Rough estimate
    };
  },

  // Sleep with jitter
  sleepWithJitter: (baseMs, jitterMs = 100) => {
    const jitter = Math.random() * jitterMs;
    return (baseMs + jitter) / 1000; // Convert to seconds for k6
  },

  // Format bytes
  formatBytes: (bytes, decimals = 2) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  },

  // Calculate percentile
  calculatePercentile: (arr, percentile) => {
    const sorted = [...arr].sort((a, b) => a - b);
    const index = Math.ceil((percentile / 100) * sorted.length) - 1;
    return sorted[index];
  }
};

// Export default configuration
export default {
  TestData,
  PerformanceThresholds,
  CommonChecks,
  Utils
};
