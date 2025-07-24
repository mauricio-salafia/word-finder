# WordFinder API Benchmark Implementation

## 🚀 **Complete K6 Benchmark Suite**

A comprehensive performance testing suite for the WordFinder API using K6, designed to validate performance, scalability, and reliability under various load conditions.

---

## 📁 **Benchmark Structure**

```
benchmark/
├── README.md                    # This documentation
├── config.js                   # Shared configuration and utilities
├── run-benchmarks.sh           # Main benchmark runner script
├── quick-test.js               # Quick validation test
├── basic-load-test.js          # Standard load testing
├── stress-test.js              # High-load stress testing
├── spike-test.js               # Traffic spike testing
├── endurance-test.js           # Long-running stability testing
└── benchmark-results/          # Test results directory
    └── README.md               # Results documentation
```

---

## 🎯 **Benchmark Test Types**

### 1. **Quick Test** (`quick-test.js`)
- **Duration**: ~1 minute
- **Purpose**: Fast validation that API is working correctly
- **Load**: 5 concurrent users
- **Use Case**: Pre-benchmark validation, CI/CD checks

### 2. **Basic Load Test** (`basic-load-test.js`)
- **Duration**: ~4 minutes
- **Purpose**: Standard performance testing under normal conditions
- **Load**: 10-25 concurrent users
- **Thresholds**: p95 < 1000ms, error rate < 1%

### 3. **Stress Test** (`stress-test.js`)
- **Duration**: ~10 minutes
- **Purpose**: Push API to limits and identify breaking points
- **Load**: 100-300 concurrent users
- **Thresholds**: p95 < 3000ms, error rate < 5%

### 4. **Spike Test** (`spike-test.js`)
- **Duration**: ~4 minutes
- **Purpose**: Test handling of sudden traffic increases
- **Load**: 20-300 users (sudden spikes)
- **Thresholds**: p95 < 2000ms, error rate < 3%

### 5. **Endurance Test** (`endurance-test.js`)
- **Duration**: ~48 minutes
- **Purpose**: Long-term stability and memory leak detection
- **Load**: 50-100 concurrent users (sustained)
- **Thresholds**: p95 < 1500ms, error rate < 2%

---

## 🛠️ **Setup and Installation**

### **Prerequisites**

#### 1. **Install K6**
```bash
# macOS
brew install k6

# Ubuntu/Debian
sudo apt-get install k6

# Windows (Chocolatey)
choco install k6

# Verify installation
k6 version
```

#### 2. **Start WordFinder API**
```bash
cd ../src/WordFinder.Api
dotnet run
```

#### 3. **Verify API is Running**
```bash
curl http://localhost:5211/api/health
```

---

## 🚀 **Running Benchmarks**

### **Quick Start**
```bash
# Navigate to benchmark directory
cd benchmark

# Run all benchmarks
./run-benchmarks.sh

# Run specific benchmark
./run-benchmarks.sh basic    # Basic load test
./run-benchmarks.sh stress   # Stress test
./run-benchmarks.sh spike    # Spike test
./run-benchmarks.sh endurance # Endurance test (48 minutes)
```

### **Individual Test Execution**
```bash
# Quick validation
k6 run quick-test.js

# Basic load test
k6 run basic-load-test.js

# Stress test
k6 run stress-test.js

# Spike test
k6 run spike-test.js

# Endurance test (long-running)
k6 run endurance-test.js
```

### **Custom Configuration**
```bash
# Custom virtual users and duration
k6 run --vus 50 --duration 30s basic-load-test.js

# Custom environment variables
k6 run --env BASE_URL=http://localhost:8080 basic-load-test.js

# Output to file
k6 run --out json=results.json basic-load-test.js
```

---

## 📊 **Test Data and Scenarios**

### **Matrix Sizes**
- **Small (5x5)**: Challenge example, fast processing
- **Medium (16x16)**: Balanced complexity
- **Large (32x32)**: High complexity, stress testing
- **Extra Large (64x64)**: Maximum supported size

### **Word Stream Variety**
- **Simple**: 3-5 common words
- **Complex**: 15-25 technical terms
- **Realistic**: Mix of found and not-found words
- **Extreme**: 50+ words for maximum processing

### **Test Distribution**
- **60%** Small matrices (fast response)
- **30%** Medium matrices (balanced load)
- **10%** Large matrices (stress testing)

---

## 🎯 **Performance Targets**

### **Response Time Thresholds**
| Scenario | p95 Target | p99 Target | Notes |
|----------|------------|------------|-------|
| Normal Load | < 1000ms | < 2000ms | Production ready |
| Stress Load | < 3000ms | < 5000ms | Acceptable under stress |
| Spike Load | < 2000ms | < 4000ms | Recovery capability |
| Endurance | < 1500ms | < 3000ms | Long-term stability |

### **Error Rate Targets**
| Scenario | Target | Maximum | Notes |
|----------|--------|---------|-------|
| Normal Load | < 1% | < 2% | Production ready |
| Stress Load | < 5% | < 10% | Under extreme load |
| Spike Load | < 3% | < 5% | During traffic spikes |
| Endurance | < 2% | < 3% | Long-term reliability |

### **Throughput Targets**
| Scenario | Minimum | Good | Excellent |
|----------|---------|------|-----------|
| Concurrent Users | 50 | 200 | 500+ |
| Requests/Second | 100 | 500 | 1000+ |
| Data Throughput | 1 MB/s | 5 MB/s | 10+ MB/s |

---

## 📈 **Results and Analysis**

### **Generated Reports**
Each benchmark run creates:
- **JSON Summary**: Machine-readable metrics
- **HTML Report**: Visual analysis with charts
- **Raw Data**: Complete K6 metrics for custom analysis

### **Key Metrics**
- **Response Time**: Average, p95, p99 percentiles
- **Error Rate**: Failed requests percentage
- **Throughput**: Requests per second
- **Data Transfer**: Sent/received bytes
- **Virtual Users**: Concurrent user simulation

### **Performance Analysis**
```bash
# View HTML report
open benchmark-results/basic-load-test_*/basic-load-test-summary.html

# Check error rates
jq '.metrics.http_req_failed.values.rate' benchmark-results/*/raw-results.json

# Compare response times
jq '.metrics.http_req_duration.values.avg' benchmark-results/*/raw-results.json
```

---

## 🔧 **Advanced Configuration**

### **Custom Test Scenarios**
```javascript
// Example: Custom load pattern
export const options = {
  stages: [
    { duration: '2m', target: 100 },
    { duration: '5m', target: 100 },
    { duration: '2m', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<1000'],
    http_req_failed: ['rate<0.01'],
  },
};
```

### **Environment Variables**
```bash
# Custom API URL
export BASE_URL=http://localhost:8080

# Custom test duration
export TEST_DURATION=300s

# Custom virtual users
export VUS=50
```

### **CI/CD Integration**
```yaml
# GitHub Actions example
- name: Run Performance Tests
  run: |
    cd benchmark
    ./run-benchmarks.sh basic
    # Check results
    if [ $(jq '.metrics.http_req_failed.values.rate' benchmark-results/*/raw-results.json) > 0.01 ]; then
      echo "Performance test failed"
      exit 1
    fi
```

---

## 🚨 **Troubleshooting**

### **Common Issues**

#### **API Connection Failed**
```bash
# Check API status
curl http://localhost:5211/api/health

# Verify API is running
cd ../src/WordFinder.Api && dotnet run
```

#### **High Error Rates**
1. Check API logs for error details
2. Monitor system resources (CPU, memory)
3. Verify database connections
4. Check network connectivity

#### **Poor Performance**
1. Review response time percentiles
2. Check for resource bottlenecks
3. Analyze garbage collection patterns
4. Monitor I/O operations

#### **K6 Installation Issues**
```bash
# Verify K6 installation
k6 version

# Reinstall if needed
brew reinstall k6  # macOS
```

### **Performance Optimization**
- **Caching**: Implement response caching
- **Connection Pooling**: Use HTTP connection pooling
- **Async Processing**: Leverage async operations
- **Resource Monitoring**: Monitor CPU, memory, and I/O

---

## 📋 **Best Practices**

### **Test Design**
- Use realistic test data
- Implement proper ramp-up periods
- Include health checks in tests
- Test error scenarios

### **Result Analysis**
- Focus on percentiles, not averages
- Monitor trends over time
- Compare against baseline
- Document performance regressions

### **Continuous Testing**
- Integrate with CI/CD pipelines
- Run tests on every release
- Monitor production metrics
- Set up alerting for regressions

---

## 📚 **Additional Resources**

### **K6 Documentation**
- [K6 Official Documentation](https://k6.io/docs/)
- [K6 Test Types](https://k6.io/docs/test-types/)
- [K6 Thresholds](https://k6.io/docs/using-k6/thresholds/)

### **Performance Testing**
- [Performance Testing Best Practices](https://k6.io/docs/testing-guides/)
- [Load Testing Patterns](https://k6.io/docs/test-types/load-testing/)
- [Stress Testing Guide](https://k6.io/docs/test-types/stress-testing/)

### **WordFinder API**
- [API Documentation](../README.md)
- [Implementation Guide](../Implementation.md)
- [Test Scripts](../test-api.sh)

---

## 🎉 **Conclusion**

This benchmark suite provides comprehensive performance testing for the WordFinder API, covering:

- ✅ **Multiple Test Types**: Load, stress, spike, and endurance testing
- ✅ **Realistic Scenarios**: Various matrix sizes and word streams
- ✅ **Detailed Reporting**: JSON and HTML reports with analysis
- ✅ **Automation Ready**: CI/CD integration and scripted execution
- ✅ **Performance Targets**: Clear thresholds and success criteria

The benchmarks help ensure the WordFinder API meets performance requirements and can handle production workloads reliably.
