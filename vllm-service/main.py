import os
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import torch
from vllm import LLM, SamplingParams
from transformers import AutoTokenizer
import logging
import uvicorn

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(title="Qwen3 Reranker API", version="1.0.0")

class RerankRequest(BaseModel):
    vacancy_title: str
    job_description: str
    instruction: str = "Given a vacancy title, retrieve relevant job description of the candidate that is suitable for the vacancy"

class RerankResponse(BaseModel):
    probability: float
    score: float

# Global variables for model and tokenizer
model = None
tokenizer = None
sampling_params = None
token_false_id = None
token_true_id = None

def format_instruction(instruction: str, query: str, doc: str) -> str:
    """Format the instruction for the model"""
    if instruction is None:
        instruction = 'Given a web search query, retrieve relevant passages that answer the query'
    output = "<Instruct>: {instruction}\n<Query>: {query}\n<Document>: {doc}".format(
        instruction=instruction, query=query, doc=doc
    )
    return output

def create_prompt(instruction: str, query: str, doc: str) -> str:
    """Create the full prompt for vLLM"""
    formatted_instruction = format_instruction(instruction, query, doc)
    
    prefix = "<|im_start|>system\nJudge whether the Document meets the requirements based on the Query and the Instruct provided. Note that the answer can only be \"yes\" or \"no\".<|im_end|>\n<|im_start|>user\n"
    suffix = "<|im_end|>\n<|im_start|>assistant\n<think>\n\n</think>\n\n"
    
    full_prompt = prefix + formatted_instruction + suffix
    return full_prompt

@app.on_event("startup")
async def startup_event():
    """Initialize the model and tokenizer on startup"""
    global model, tokenizer, sampling_params, token_false_id, token_true_id
    
    logger.info("Loading model and tokenizer for CPU inference...")

    # Read model directory from environment or use default
    model_dir = os.getenv("MODEL_DIR", "/models/Qwen3-Reranker-0.6B")
    logger.info(f"Using model directory: {model_dir}")
    
    try:
        # Initialize tokenizer from local path
        tokenizer = AutoTokenizer.from_pretrained(
            model_dir,
            trust_remote_code=True,
            local_files_only=True
        )
        
        # Initialize vLLM model for CPU inference
        model = LLM(
            model=model_dir,
            tensor_parallel_size=1,
            # Remove GPU-specific settings
            # gpu_memory_utilization=0.8,
            max_model_len=4096,  # Reduced for CPU inference
            trust_remote_code=True,
            # Add CPU-specific settings
            device="cpu",
            dtype="float32",  # Use float32 for CPU
            # Disable GPU options
            enforce_eager=True,
            disable_custom_all_reduce=True
        )
        
        # Get token IDs
        token_false_id = tokenizer.convert_tokens_to_ids("no")
        token_true_id = tokenizer.convert_tokens_to_ids("yes")
        
        # Configure sampling parameters for logit extraction
        sampling_params = SamplingParams(
            temperature=0.0,
            max_tokens=1,
            logprobs=2,  # Get logprobs for top 2 tokens
            prompt_logprobs=0
        )
        
        logger.info("Model and tokenizer loaded successfully for CPU inference")
        
    except Exception as e:
        logger.error(f"Failed to load model: {str(e)}")
        raise e

def extract_probability_from_logprobs(outputs) -> float:
    """Extract probability from vLLM output logprobs"""
    try:
        output = outputs[0]
        if not output.outputs:
            return 0.0
            
        # Get the logprobs for the first (and only) token
        token_logprobs = output.outputs[0].logprobs
        if not token_logprobs:
            return 0.0
            
        first_token_logprobs = token_logprobs[0]
        
        # Get logprobs for 'yes' and 'no' tokens
        yes_logprob = None
        no_logprob = None
        
        for token_id, logprob_data in first_token_logprobs.items():
            if token_id == token_true_id:
                yes_logprob = logprob_data.logprob
            elif token_id == token_false_id:
                no_logprob = logprob_data.logprob
        
        # If we don't have both logprobs, try to get them from the model's vocabulary
        if yes_logprob is None or no_logprob is None:
            # Fallback: assume very low probability for missing tokens
            if yes_logprob is None:
                yes_logprob = -10.0
            if no_logprob is None:
                no_logprob = -10.0
        
        # Compute softmax probability
        import math
        yes_prob = math.exp(yes_logprob)
        no_prob = math.exp(no_logprob)
        total_prob = yes_prob + no_prob
        
        if total_prob == 0:
            return 0.0
            
        probability = yes_prob / total_prob
        return probability
        
    except Exception as e:
        logger.error(f"Error extracting probability: {str(e)}")
        return 0.0

@app.post("/rerank", response_model=RerankResponse)
async def rerank(request: RerankRequest):
    """Rerank a job description against a vacancy title"""
    if model is None or tokenizer is None:
        raise HTTPException(status_code=500, detail="Model not initialized")
    
    try:
        # Create the prompt
        prompt = create_prompt(request.instruction, request.vacancy_title, request.job_description)
        
        # Generate with vLLM
        outputs = model.generate([prompt], sampling_params)
        
        # Extract probability
        probability = extract_probability_from_logprobs(outputs)
        
        # Calculate score (log probability)
        import math
        score = math.log(max(probability, 1e-10))  # Avoid log(0)
        
        logger.info(f"Processed request - Probability: {probability:.4f}, Score: {score:.4f}")
        
        return RerankResponse(probability=probability, score=score)
        
    except Exception as e:
        logger.error(f"Error processing request: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Error processing request: {str(e)}")

@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "model_loaded": model is not None}

@app.get("/")
async def root():
    """Root endpoint"""
    return {"message": "Qwen3 Reranker API", "version": "1.0.0"}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
