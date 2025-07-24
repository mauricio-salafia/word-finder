#!/bin/bash

# WordFinder API Benchmark Runner
# This script runs all benchmark tests and generates comprehensive reports

set -e

# Configuration
API_BASE_URL="http://localhost:5211"
BENCHMARK_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS_DIR="$BENCHMARK_DIR/benchmark-results"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== WordFinder API Benchmark Suite ===${NC}"
echo -e "Timestamp: ${TIMESTAMP}"
echo -e "API Base URL: ${API_BASE_URL}"
echo -e "Results Directory: ${RESULTS_DIR}"
echo

# Function to check if API is running
check_api() {
    echo -e "${YELLOW}Checking if API is running...${NC}"
    if curl -s "${API_BASE_URL}/api/health" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ API is running${NC}"
        return 0
    else
        echo -e "${RED}❌ API is not running${NC}"
        echo -e "${YELLOW}Please start the API first:${NC}"
        echo -e "cd ../src/WordFinder.Api && dotnet run"
        return 1
    fi
}

# Function to check if k6 is installed
check_k6() {
    echo -e "${YELLOW}Checking K6 installation...${NC}"
    if command -v k6 > /dev/null 2>&1; then
        echo -e "${GREEN}✅ K6 is installed: $(k6 version)${NC}"
        return 0
    else
        echo -e "${RED}❌ K6 is not installed${NC}"
        echo -e "${YELLOW}Please install K6 first:${NC}"
        echo -e "macOS: brew install k6"
        echo -e "Ubuntu: sudo apt-get install k6"
        echo -e "Windows: choco install k6"
        return 1
    fi
}

# Function to run a specific benchmark
run_benchmark() {
    local test_name=$1
    local test_file=$2
    local description=$3
    
    echo
    echo -e "${BLUE}=== Running ${test_name} ===${NC}"
    echo -e "${YELLOW}${description}${NC}"
    echo
    
    # Create timestamped results directory
    local test_results_dir="${RESULTS_DIR}/${test_name}_${TIMESTAMP}"
    mkdir -p "$test_results_dir"
    
    # Run the test
    cd "$BENCHMARK_DIR"
    if BASE_URL="$API_BASE_URL" k6 run --out json="${test_results_dir}/raw-results.json" "$test_file"; then
        echo -e "${GREEN}✅ ${test_name} completed successfully${NC}"
        
        # Move generated reports to timestamped directory
        if [ -f "${RESULTS_DIR}/${test_name}-summary.json" ]; then
            mv "${RESULTS_DIR}/${test_name}-summary.json" "${test_results_dir}/"
        fi
        if [ -f "${RESULTS_DIR}/${test_name}-summary.html" ]; then
            mv "${RESULTS_DIR}/${test_name}-summary.html" "${test_results_dir}/"
        fi
        
        return 0
    else
        echo -e "${RED}❌ ${test_name} failed${NC}"
        return 1
    fi
}

# Function to generate summary report
generate_summary() {
    local summary_file="${RESULTS_DIR}/benchmark-summary_${TIMESTAMP}.md"
    
    echo -e "${YELLOW}Generating benchmark summary...${NC}"
    
    cat > "$summary_file" << EOF
# WordFinder API Benchmark Summary

**Test Date:** $(date)
**API Base URL:** ${API_BASE_URL}
**Test Duration:** Full benchmark suite

## Test Results

### Basic Load Test
- **Purpose:** Validate API performance under normal load
- **Results:** See \`basic-load-test_${TIMESTAMP}/\`

### Stress Test
- **Purpose:** Identify performance limits and breaking points
- **Results:** See \`stress-test_${TIMESTAMP}/\`

### Spike Test
- **Purpose:** Test handling of sudden traffic increases
- **Results:** See \`spike-test_${TIMESTAMP}/\`

### Endurance Test
- **Purpose:** Validate long-term stability and memory management
- **Results:** See \`endurance-test_${TIMESTAMP}/\`

## Files Generated

Each test creates the following files:
- \`raw-results.json\` - Raw K6 metrics data
- \`*-summary.json\` - Processed summary metrics
- \`*-summary.html\` - HTML report with visualizations

## Analysis

### Performance Thresholds
- **Response Time (p95):** < 1000ms for normal operations
- **Error Rate:** < 1% for production readiness
- **Throughput:** > 100 requests/second minimum

### Recommendations
1. Review HTML reports for detailed analysis
2. Monitor memory usage during endurance tests
3. Check error rates during stress and spike tests
4. Verify response time consistency across all tests

### Next Steps
1. Address any performance bottlenecks identified
2. Optimize based on stress test results
3. Implement monitoring based on benchmark metrics
4. Schedule regular performance regression testing

EOF

    echo -e "${GREEN}✅ Summary report generated: ${summary_file}${NC}"
}

# Main execution
main() {
    echo -e "${BLUE}Starting benchmark suite...${NC}"
    
    # Pre-flight checks
    if ! check_k6; then
        exit 1
    fi
    
    if ! check_api; then
        exit 1
    fi
    
    # Ensure results directory exists
    mkdir -p "$RESULTS_DIR"
    
    # Run benchmarks
    local failed_tests=0
    
    if ! run_benchmark "basic-load-test" "basic-load-test.js" "Tests API performance under normal load conditions"; then
        ((failed_tests++))
    fi
    
    if ! run_benchmark "stress-test" "stress-test.js" "Tests API performance under high stress conditions"; then
        ((failed_tests++))
    fi
    
    if ! run_benchmark "spike-test" "spike-test.js" "Tests API response to sudden traffic spikes"; then
        ((failed_tests++))
    fi
    
    # Ask user if they want to run endurance test (takes ~48 minutes)
    echo
    echo -e "${YELLOW}The endurance test takes approximately 48 minutes to complete.${NC}"
    read -p "Do you want to run the endurance test? (y/N): " run_endurance
    if [[ $run_endurance =~ ^[Yy]$ ]]; then
        if ! run_benchmark "endurance-test" "endurance-test.js" "Tests long-term stability and memory management"; then
            ((failed_tests++))
        fi
    else
        echo -e "${YELLOW}Skipping endurance test${NC}"
    fi
    
    # Generate summary
    generate_summary
    
    # Final results
    echo
    echo -e "${BLUE}=== Benchmark Suite Complete ===${NC}"
    if [ $failed_tests -eq 0 ]; then
        echo -e "${GREEN}✅ All tests completed successfully${NC}"
    else
        echo -e "${RED}❌ ${failed_tests} test(s) failed${NC}"
    fi
    
    echo -e "${YELLOW}Results saved to: ${RESULTS_DIR}/${NC}"
    echo -e "${YELLOW}View HTML reports for detailed analysis${NC}"
}

# Handle script arguments
case "${1:-}" in
    "basic")
        check_k6 && check_api && run_benchmark "basic-load-test" "basic-load-test.js" "Basic load test"
        ;;
    "stress")
        check_k6 && check_api && run_benchmark "stress-test" "stress-test.js" "Stress test"
        ;;
    "spike")
        check_k6 && check_api && run_benchmark "spike-test" "spike-test.js" "Spike test"
        ;;
    "endurance")
        check_k6 && check_api && run_benchmark "endurance-test" "endurance-test.js" "Endurance test"
        ;;
    "help"|"-h"|"--help")
        echo "Usage: $0 [test-type]"
        echo "  test-type: basic, stress, spike, endurance"
        echo "  If no test-type is provided, runs all tests"
        ;;
    *)
        main
        ;;
esac
