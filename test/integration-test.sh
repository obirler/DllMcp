#!/bin/bash

# Integration test script for MCP-DLL Document Proxy Server

set -e

echo "Starting MCP-DLL Document Proxy Server tests..."

# Start the server in the background
cd /home/runner/work/DllMcp/DllMcp/src/DllMcp.Api
dotnet run --urls "http://localhost:5000" &
SERVER_PID=$!

# Wait for server to start
echo "Waiting for server to start..."
sleep 5

# Function to cleanup on exit
cleanup() {
    echo "Cleaning up..."
    kill $SERVER_PID 2>/dev/null || true
    wait $SERVER_PID 2>/dev/null || true
}
trap cleanup EXIT

# Test 1: Upload assembly
echo ""
echo "Test 1: Uploading TestAssembly.dll with XML documentation..."
UPLOAD_RESPONSE=$(curl -s -X POST http://localhost:5000/api/assemblies/upload \
  -F "dll=@/home/runner/work/DllMcp/DllMcp/test/TestAssembly/bin/Release/net9.0/TestAssembly.dll" \
  -F "xml=@/home/runner/work/DllMcp/DllMcp/test/TestAssembly/bin/Release/net9.0/TestAssembly.xml")

echo "Upload response: $UPLOAD_RESPONSE"

# Extract assembly ID
ASSEMBLY_ID=$(echo $UPLOAD_RESPONSE | grep -o '"assemblyId":"[^"]*"' | cut -d'"' -f4)
echo "Assembly ID: $ASSEMBLY_ID"

if [ -z "$ASSEMBLY_ID" ]; then
    echo "❌ Failed to upload assembly"
    exit 1
fi
echo "✅ Assembly uploaded successfully"

# Test 2: List assemblies
echo ""
echo "Test 2: Listing all assemblies..."
ASSEMBLIES=$(curl -s http://localhost:5000/api/assemblies)
echo "Assemblies: $ASSEMBLIES"

if echo "$ASSEMBLIES" | grep -q "TestAssembly"; then
    echo "✅ Assembly listing successful"
else
    echo "❌ Assembly not found in listing"
    exit 1
fi

# Test 3: List types
echo ""
echo "Test 3: Listing types from assembly..."
TYPES=$(curl -s "http://localhost:5000/api/assemblies/$ASSEMBLY_ID/types?pageSize=50")
echo "Types response: $TYPES"

if echo "$TYPES" | grep -q "Calculator"; then
    echo "✅ Calculator class found"
else
    echo "❌ Calculator class not found"
    exit 1
fi

if echo "$TYPES" | grep -q "StringHelper"; then
    echo "✅ StringHelper class found"
else
    echo "❌ StringHelper class not found"
    exit 1
fi

# Test 4: Search types
echo ""
echo "Test 4: Searching for 'Calculator'..."
SEARCH_RESULTS=$(curl -s "http://localhost:5000/api/assemblies/$ASSEMBLY_ID/types?search=Calculator&pageSize=50")
echo "Search results: $SEARCH_RESULTS"

if echo "$SEARCH_RESULTS" | grep -q "Calculator"; then
    echo "✅ Search functionality working"
else
    echo "❌ Search functionality failed"
    exit 1
fi

# Test 5: MCP Tools - List Types
echo ""
echo "Test 5: Testing MCP dll.listTypes tool..."
MCP_TYPES=$(curl -s -X POST http://localhost:5000/mcp/dll.listTypes \
  -H "Content-Type: application/json" \
  -d "{\"assemblyId\":\"$ASSEMBLY_ID\",\"pageSize\":50}")
echo "MCP Types response: $MCP_TYPES"

if echo "$MCP_TYPES" | grep -q "Calculator"; then
    echo "✅ MCP dll.listTypes working"
else
    echo "❌ MCP dll.listTypes failed"
    exit 1
fi

echo ""
echo "================================"
echo "✅ All tests passed successfully!"
echo "================================"
