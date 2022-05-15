docker build -t sample:prod .
docker run -it --rm -p 1234:80 sample:prod