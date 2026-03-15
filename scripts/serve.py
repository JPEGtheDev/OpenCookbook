#!/usr/bin/env python3
"""serve.py — Local preview server for OpenCookbook Blazor WASM.

Run from the extracted preview folder:
    python3 serve.py          # serves on http://localhost:8000
    python3 serve.py 9000     # serves on a custom port

Unknown paths fall back to index.html so Blazor client-side routing works.
"""
import http.server
import mimetypes
import os
import socketserver
import sys

PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 8000

# Ensure .wasm files get the correct MIME type (required by browsers)
mimetypes.add_type("application/wasm", ".wasm")


class SPAHandler(http.server.SimpleHTTPRequestHandler):
    """Serve static files; fall back to index.html for unknown paths."""

    def do_GET(self):
        path = self.translate_path(self.path)
        if not os.path.exists(path):
            self.path = "/index.html"
        return super().do_GET()

    def log_message(self, format, *args):
        pass  # Suppress per-request logs for cleaner output


with socketserver.TCPServer(("", PORT), SPAHandler) as httpd:
    httpd.allow_reuse_address = True
    print(f"OpenCookbook preview → http://localhost:{PORT}")
    print("Press Ctrl+C to stop.")
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\nStopped.")
