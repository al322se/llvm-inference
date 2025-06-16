# llvm-inference

This project provides a simple example of running the Qwen3 reranker model with [vLLM](https://github.com/vllm-project/vllm) and a small ASP.NET web application.

## Local model

The Python service expects the model and tokenizer files to be available locally. Copy the contents of the `Qwen/Qwen3-Reranker-0.6B` repository into:

```
models/Qwen3-Reranker-0.6B
```

Docker mounts this directory into the container at `/models` and the service loads the model from `MODEL_DIR=/models/Qwen3-Reranker-0.6B`.

## Building the images

To build the Python service image manually run:

```bash
docker build -t llvm-vllm-service ./vllm-service
```

The web application can be built with Docker Compose or individually in a similar way.

## Running locally

After placing the model files, start the stack with Docker Compose:

```bash
docker compose up --build
```

This starts the `vllm-service` on port `8000` and the ASP.NET frontend on port `8080`.
