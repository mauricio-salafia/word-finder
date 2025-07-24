# WordFinder API Benchmarks

This folder contains K6 performance benchmarks for the WordFinder API.

## Overview

The benchmarks test the performance of the WordFinder API under various loads and scenarios using [K6](https://k6.io/), a modern load testing tool.

## Prerequisites

1. **Install K6**:
   ```bash
   # macOS
   brew install k6
   
   # Ubuntu/Debian
   sudo apt-get install k6
   
   # Windows (using Chocolatey)
   choco install k6
   ```

2. **Start the WordFinder API**:
   ```bash
   cd ../src/WordFinder.Api
   dotnet run
   ```

## Benchmark Scripts

### 1. **basic-load-test.js**
- Tests basic API functionality under moderate load
- Validates response times and success rates
- Tests both health check and word search endpoints

### 2. **stress-test.js**
- High-load stress testing
- Tests API behavior under extreme conditions
- Identifies breaking points and performance limits

### 3. **spike-test.js**
- Tests API response to sudden traffic spikes
- Validates auto-scaling and recovery capabilities
- Simulates real-world traffic patterns

### 4. **endurance-test.js**
- Long-running endurance testing
- Tests for memory leaks and performance degradation
- Validates system stability over time

## Running Benchmarks

### Quick Start
```bash
# Run basic load test
k6 run basic-load-test.js

# Run stress test
k6 run stress-test.js

# Run spike test
k6 run spike-test.js

# Run endurance test
k6 run endurance-test.js
```

### Custom Configuration
```bash
# Run with custom virtual users and duration
k6 run --vus 50 --duration 30s basic-load-test.js

# Run with custom thresholds
k6 run --env BASE_URL=http://localhost:5211 basic-load-test.js
```

## Test Data

The benchmarks use realistic test data that matches the WordFinder algorithm requirements:

- **Small Matrix**: 5x5 character matrix (similar to challenge example)
- **Medium Matrix**: 16x16 character matrix 
- **Large Matrix**: 32x32 character matrix
- **Various Word Streams**: Different sizes and complexity levels

## Expected Results

### Performance Targets
- **Response Time**: < 100ms for small matrices (5x5)
- **Response Time**: < 500ms for medium matrices (16x16)
- **Response Time**: < 2000ms for large matrices (32x32)
- **Success Rate**: 99%+ under normal load
- **Throughput**: 1000+ requests per second

### Scalability Metrics
- **Concurrent Users**: Support for 100+ concurrent users
- **Memory Usage**: Stable memory consumption under load
- **CPU Usage**: Efficient CPU utilization
- **Error Rate**: < 1% under normal conditions

## Report Generation

K6 generates detailed performance reports including:

- **Response Time Distribution**: Percentiles and histograms
- **Throughput Metrics**: Requests per second over time
- **Error Analysis**: Failed requests and error types
- **Resource Utilization**: System performance metrics

## Integration with CI/CD

The benchmarks can be integrated into CI/CD pipelines:

```yaml
# Example GitHub Actions workflow
- name: Run Performance Tests
  run: |
    cd benchmark
    k6 run --quiet basic-load-test.js
```

## Troubleshooting

### Common Issues
1. **Connection Refused**: Ensure the API is running on the correct port
2. **High Response Times**: Check system resources and API configuration
3. **K6 Not Found**: Verify K6 installation and PATH configuration

### Performance Optimization
- Use connection pooling for better performance
- Implement caching strategies for repeated requests
- Monitor system resources during tests
- Adjust virtual user ramp-up rates based on system capacity
