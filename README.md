# Build the image
docker build -t pdfreader:latest .

# Run the container
docker run -p 5038:8080 pdfreader:latest