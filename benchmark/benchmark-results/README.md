# Benchmark Results Directory

This directory contains the results of K6 benchmark tests for the WordFinder API.

## File Structure

Each benchmark run creates a timestamped directory with the following files:

```
benchmark-results/
├── basic-load-test_YYYYMMDD_HHMMSS/
│   ├── raw-results.json          # Raw K6 metrics data
│   ├── basic-load-test-summary.json  # Processed summary
│   └── basic-load-test-summary.html  # HTML report
├── stress-test_YYYYMMDD_HHMMSS/
│   ├── raw-results.json
│   ├── stress-test-summary.json
│   └── stress-test-summary.html
├── spike-test_YYYYMMDD_HHMMSS/
│   ├── raw-results.json
│   ├── spike-test-summary.json
│   └── spike-test-summary.html
├── endurance-test_YYYYMMDD_HHMMSS/
│   ├── raw-results.json
│   ├── endurance-test-summary.json
│   └── endurance-test-summary.html
└── benchmark-summary_YYYYMMDD_HHMMSS.md  # Overall summary
```

## File Types

### Raw Results (raw-results.json)
- Complete K6 metrics data in JSON format
- Includes all timing, throughput, and error data
- Can be used for custom analysis and reporting

### Summary Files (JSON)
- Processed summary of key metrics
- Machine-readable format for automation
- Includes performance thresholds and pass/fail status

### HTML Reports
- Human-readable performance reports
- Visual charts and graphs
- Detailed analysis and recommendations
- Color-coded results (green=pass, yellow=warning, red=fail)

## Viewing Results

### HTML Reports
Open the HTML files in your web browser for detailed visual analysis:
```bash
open benchmark-results/basic-load-test_YYYYMMDD_HHMMSS/basic-load-test-summary.html
```

### JSON Analysis
Use jq or other tools to analyze JSON results:
```bash
# View summary metrics
jq '.metrics.http_req_duration' benchmark-results/basic-load-test_*/raw-results.json

# Check error rates
jq '.metrics.http_req_failed.values.rate' benchmark-results/*/raw-results.json
```

### Command Line Summary
```bash
# View the overall benchmark summary
cat benchmark-results/benchmark-summary_YYYYMMDD_HHMMSS.md
```

## Performance Targets

### Response Time Targets
- **p95 < 1000ms**: Normal operations
- **p95 < 2000ms**: Acceptable under stress
- **p95 < 3000ms**: Maximum acceptable

### Error Rate Targets
- **< 1%**: Production ready
- **< 3%**: Acceptable for most scenarios
- **< 5%**: Maximum acceptable under extreme load

### Throughput Targets
- **> 100 req/s**: Minimum acceptable
- **> 500 req/s**: Good performance
- **> 1000 req/s**: Excellent performance

## Troubleshooting

### High Error Rates
1. Check API logs for error details
2. Verify API is running and accessible
3. Monitor system resources (CPU, memory)
4. Check database connections if applicable

### Poor Performance
1. Review response time percentiles
2. Check for resource bottlenecks
3. Analyze garbage collection patterns
4. Monitor network latency

### Test Failures
1. Verify API endpoint availability
2. Check test data validity
3. Review K6 configuration
4. Validate network connectivity

## Automation

The benchmark results can be integrated into CI/CD pipelines:

```yaml
# Example GitHub Actions step
- name: Run Performance Tests
  run: |
    cd benchmark
    ./run-benchmarks.sh basic
    # Parse results and fail if thresholds not met
    if [ $(jq '.metrics.http_req_failed.values.rate' benchmark-results/*/raw-results.json) > 0.01 ]; then
      echo "Error rate too high"
      exit 1
    fi
```

## Historical Analysis

Track performance trends over time by comparing results:

```bash
# Compare average response times
echo "Historical Response Times:"
for dir in benchmark-results/basic-load-test_*/; do
  timestamp=$(basename "$dir" | cut -d'_' -f2-)
  avg_time=$(jq '.metrics.http_req_duration.values.avg' "$dir/raw-results.json")
  echo "$timestamp: ${avg_time}ms"
done
```
