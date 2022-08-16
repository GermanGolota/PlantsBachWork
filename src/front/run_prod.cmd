docker build -t sample:prod .
docker run -it --rm -p 1235:80 sample:prod